﻿using System;
using System.Collections.Generic;
using System.Linq;
using Cortex.BeaconNode.Configuration;
using Cortex.BeaconNode.Ssz;
using Cortex.Containers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cortex.BeaconNode
{
    public class BeaconStateTransition
    {
        private readonly BeaconChainUtility _beaconChainUtility;
        private readonly BeaconStateAccessor _beaconStateAccessor;
        private readonly IOptionsMonitor<InitialValues> _initialValueOptions;
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<MaxOperationsPerBlock> _maxOperationsPerBlock;
        private readonly ICryptographyService _cryptographyService;
        private readonly IOptionsMonitor<MiscellaneousParameters> _miscellaneousParameterOptions;
        private readonly IOptionsMonitor<StateListLengths> _stateListLengthOptions;
        private readonly IOptionsMonitor<TimeParameters> _timeParameterOptions;

        public BeaconStateTransition(ILogger<BeaconStateTransition> logger,
            IOptionsMonitor<MiscellaneousParameters> miscellaneousParameterOptions,
            IOptionsMonitor<InitialValues> initialValueOptions,
            IOptionsMonitor<TimeParameters> timeParameterOptions,
            IOptionsMonitor<StateListLengths> stateListLengthOptions,
            IOptionsMonitor<MaxOperationsPerBlock> maxOperationsPerBlock,
            ICryptographyService cryptographyService,
            BeaconChainUtility beaconChainUtility,
            BeaconStateAccessor beaconStateAccessor)
        {
            _logger = logger;
            _miscellaneousParameterOptions = miscellaneousParameterOptions;
            _initialValueOptions = initialValueOptions;
            _timeParameterOptions = timeParameterOptions;
            _stateListLengthOptions = stateListLengthOptions;
            _maxOperationsPerBlock = maxOperationsPerBlock;
            _cryptographyService = cryptographyService;
            _beaconChainUtility = beaconChainUtility;
            _beaconStateAccessor = beaconStateAccessor;
        }

        public Gwei GetAttestingBalance(BeaconState state, IEnumerable<PendingAttestation> attestations)
        {
            var unslashed = GetUnslashedAttestingIndices(state, attestations);
            return _beaconStateAccessor.GetTotalBalance(state, unslashed);
        }

        public IEnumerable<PendingAttestation> GetMatchingSourceAttestations(BeaconState state, Epoch epoch)
        {
            var currentEpoch = _beaconStateAccessor.GetCurrentEpoch(state);
            var previousEpoch = _beaconStateAccessor.GetPreviousEpoch(state);
            if (epoch != currentEpoch && epoch != previousEpoch)
            {
                throw new ArgumentOutOfRangeException(nameof(epoch), epoch, $"The epoch for attestions must be either the current epoch {currentEpoch} or previous epoch {previousEpoch}.");
            }

            if (epoch == currentEpoch)
            {
                return state.CurrentEpochAttestations;
            }
            return state.PreviousEpochAttestations;
        }

        public IEnumerable<PendingAttestation> GetMatchingTargetAttestations(BeaconState state, Epoch epoch)
        {
            var blockRoot = _beaconStateAccessor.GetBlockRoot(state, epoch);
            var sourceAttestations = GetMatchingSourceAttestations(state, epoch);
            return sourceAttestations.Where(x => x.Data.Target.Root == blockRoot);
        }

        public IEnumerable<ValidatorIndex> GetUnslashedAttestingIndices(BeaconState state, IEnumerable<PendingAttestation> attestations)
        {
            IEnumerable<ValidatorIndex> output = new List<ValidatorIndex>();
            foreach (var attestation in attestations)
            {
                var attestingIndices = _beaconStateAccessor.GetAttestingIndices(state, attestation.Data, attestation.AggregationBits);
                output = output.Union(attestingIndices);
            }
            return output.Where(x => !state.Validators[(int)(ulong)x].IsSlashed);
        }

        public void ProcessBlock(BeaconState state, BeaconBlock block)
        {
            ProcessBlockHeader(state, block);
            ProcessRandao(state, block.Body);
            ProcessEth1Data(state, block.Body);
            ProcessOperations(state, block.Body);
        }

        public void ProcessBlockHeader(BeaconState state, BeaconBlock block)
        {
            // Verify that the slots match
            if (block.Slot != state.Slot)
            {
                throw new ArgumentOutOfRangeException("block.Slot", block.Slot, $"Block slot must match state slot {state.Slot}.");
            }
            // Verify that the parent matches
            var latestBlockSigningRoot = state.LatestBlockHeader.SigningRoot();
            if (block.ParentRoot != latestBlockSigningRoot)
            {
                throw new ArgumentOutOfRangeException("block.ParentRoot", block.ParentRoot, $"Block parent root must match latest block header root {latestBlockSigningRoot}.");
            }

            // Save current block as the new latest block
            var bodyRoot = block.Body.HashTreeRoot(_miscellaneousParameterOptions.CurrentValue, _maxOperationsPerBlock.CurrentValue);
            var newBlockHeader = new BeaconBlockHeader(block.Slot, 
                block.ParentRoot, 
                Hash32.Zero, // `state_root` is zeroed and overwritten in the next `process_slot` call
                bodyRoot,
                new BlsSignature() //`signature` is zeroed
                );

            // Verify proposer is not slashed
            var beaconProposerIndex = _beaconStateAccessor.GetBeaconProposerIndex(state);
            var proposer = state.Validators[(int)(ulong)beaconProposerIndex];
            if (proposer.IsSlashed)
            {
                throw new Exception("Beacon proposer must not be slashed.");
            }

            // Verify proposer signature
            var signingRoot = block.SigningRoot(_miscellaneousParameterOptions.CurrentValue, _maxOperationsPerBlock.CurrentValue);
            var domain = _beaconStateAccessor.GetDomain(state, DomainType.BeaconProposer, Epoch.None);
            var validSignature = _cryptographyService.BlsVerify(proposer.PublicKey, signingRoot, block.Signature, domain);
            if (!validSignature)
            {
                throw new Exception($"Block signature must match proposer public key ${proposer.PublicKey}");
            }
        }

        public void ProcessCrosslinks(BeaconState state)
        {
            //throw new NotImplementedException();
        }

        public void ProcessEpoch(BeaconState state)
        {
            _logger.LogDebug(Event.ProcessEpoch, "Process end of epoch for state {BeaconState}", state);
            ProcessJustificationAndFinalization(state);
            
            // Did this change ???
            //ProcessCrosslinks(state);

            // TODO:
            // ProcessRewardsAndPenalties(state);
            // ProcessRegistryUpdates(state);
            // ProcessSlashings(state);
            // ProcessFinalUpdates(state);

            // throw new NotImplementedException();
        }

        public void ProcessEth1Data(BeaconState state, BeaconBlockBody body)
        {
            //throw new NotImplementedException();
        }

        public void ProcessJustificationAndFinalization(BeaconState state)
        {
            _logger.LogDebug(Event.ProcessJustificationAndFinalization, "Process justification and finalization state {BeaconState}", state);
            var currentEpoch = _beaconStateAccessor.GetCurrentEpoch(state);
            if (currentEpoch <= _initialValueOptions.CurrentValue.GenesisEpoch + new Epoch(1))
            {
                return;
                //throw new ArgumentOutOfRangeException(nameof(state), currentEpoch, "Current epoch of state must be more than one away from genesis epoch.");
            }

            var previousEpoch = _beaconStateAccessor.GetPreviousEpoch(state);
            var oldPreviousJustifiedCheckpoint = state.PreviousJustifiedCheckpoint;
            var oldCurrentJustifiedCheckpoint = state.CurrentJustifiedCheckpoint;

            // Process justifications
            state.SetPreviousJustifiedCheckpoint(state.CurrentJustifiedCheckpoint);
            state.JustificationBitsShift();

            // Previous Epoch
            var matchingTargetAttestationsPreviousEpoch = GetMatchingTargetAttestations(state, previousEpoch);
            var attestingBalancePreviousEpoch = GetAttestingBalance(state, matchingTargetAttestationsPreviousEpoch);
            var totalActiveBalance = _beaconStateAccessor.GetTotalActiveBalance(state);
            if (attestingBalancePreviousEpoch * 3 >= totalActiveBalance * 2)
            {
                var blockRoot = _beaconStateAccessor.GetBlockRoot(state, previousEpoch);
                var checkpoint = new Checkpoint(previousEpoch, blockRoot);
                state.SetCurrentJustifiedCheckpoint(checkpoint);
                state.JustificationBits.Set(1, true);
            }

            // Current Epoch
            var matchingTargetAttestationsCurrentEpoch = GetMatchingTargetAttestations(state, currentEpoch);
            var attestingBalanceCurrentEpoch = GetAttestingBalance(state, matchingTargetAttestationsCurrentEpoch);
            if (attestingBalanceCurrentEpoch * 3 >= totalActiveBalance * 2)
            {
                var blockRoot = _beaconStateAccessor.GetBlockRoot(state, currentEpoch);
                var checkpoint = new Checkpoint(currentEpoch, blockRoot);
                state.SetCurrentJustifiedCheckpoint(checkpoint);
                state.JustificationBits.Set(0, true);
            }

            // Process finalizations
            var bits = state.JustificationBits;
            // The 2nd/3rd/4th most recent epochs are justified, the 2nd using the 4th as source
            if ((oldPreviousJustifiedCheckpoint.Epoch + new Epoch(3) == currentEpoch)
                && bits.Cast<bool>().Skip(1).Take(3).All(x => x))
            {
                state.SetFinalizedCheckpoint(oldPreviousJustifiedCheckpoint);
            }
            // The 2nd/3rd most recent epochs are justified, the 2nd using the 3rd as source
            if ((oldPreviousJustifiedCheckpoint.Epoch + new Epoch(2) == currentEpoch)
                && bits.Cast<bool>().Skip(1).Take(2).All(x => x))
            {
                state.SetFinalizedCheckpoint(oldPreviousJustifiedCheckpoint);
            }

            // The 1st/2nd/3rd most recent epochs are justified, the 1st using the 3rd as source
            if ((oldCurrentJustifiedCheckpoint.Epoch + new Epoch(2) == currentEpoch)
                && bits.Cast<bool>().Take(3).All(x => x))
            {
                state.SetFinalizedCheckpoint(oldCurrentJustifiedCheckpoint);
            }
            // The 1st/2nd most recent epochs are justified, the 1st using the 2nd as source
            if ((oldCurrentJustifiedCheckpoint.Epoch + new Epoch(1) == currentEpoch)
                && bits.Cast<bool>().Take(2).All(x => x))
            {
                state.SetFinalizedCheckpoint(oldCurrentJustifiedCheckpoint);
            }
        }

        public void ProcessOperations(BeaconState state, BeaconBlockBody body)
        {
            //throw new NotImplementedException();
        }

        public void ProcessRandao(BeaconState state, BeaconBlockBody body)
        {
            //throw new NotImplementedException();
        }

        public void ProcessSlot(BeaconState state)
        {
            _logger.LogDebug(Event.ProcessSlot, "Process current slot for state {BeaconState}", state);
            // Cache state root
            var previousStateRoot = state.HashTreeRoot(_miscellaneousParameterOptions.CurrentValue, _timeParameterOptions.CurrentValue,
                _stateListLengthOptions.CurrentValue, _maxOperationsPerBlock.CurrentValue);
            var previousRootIndex = state.Slot % _timeParameterOptions.CurrentValue.SlotsPerHistoricalRoot;
            state.SetStateRoot(previousRootIndex, previousStateRoot);
            // Cache latest block header state root
            if (state.LatestBlockHeader.StateRoot == Hash32.Zero)
            {
                state.LatestBlockHeader.SetStateRoot(previousStateRoot);
            }
            // Cache block root
            var previousBlockRoot = state.LatestBlockHeader.SigningRoot();
            state.SetBlockRoot(previousRootIndex, previousBlockRoot);
        }

        public void ProcessSlots(BeaconState state, Slot slot)
        {
            _logger.LogDebug(Event.ProcessSlots, "Process slots to {Slot} for state {BeaconState}", slot, state);
            if (state.Slot > slot)
            {
                throw new ArgumentOutOfRangeException(nameof(slot), slot, $"Slot to process should be greater than current state slot {state.Slot}");
            }

            while (state.Slot < slot)
            {
                ProcessSlot(state);
                // Process epoch on the start slot of the next epoch
                if ((state.Slot + new Slot(1)) % _timeParameterOptions.CurrentValue.SlotsPerEpoch == Slot.Zero)
                {
                    ProcessEpoch(state);
                }
                state.IncreaseSlot();
            }
        }

        public BeaconState StateTransition(BeaconState state, BeaconBlock block, bool validateStateRoot)
        {
            // Process slots (including those with no blocks) since block
            ProcessSlots(state, block.Slot);
            // Process block
            ProcessBlock(state, block);
            // Validate state root (True in production)
            if (validateStateRoot)
            {
                var checkStateRoot = state.HashTreeRoot(_miscellaneousParameterOptions.CurrentValue, _timeParameterOptions.CurrentValue, _stateListLengthOptions.CurrentValue, _maxOperationsPerBlock.CurrentValue);
                if (block.StateRoot != checkStateRoot)
                {
                    throw new Exception("Mismatch between calculated state root and block state root.");
                }
            }
            return state;
        }
    }
}
