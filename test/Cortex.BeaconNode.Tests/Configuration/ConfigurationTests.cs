﻿using Cortex.BeaconNode.Configuration;
using Cortex.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Cortex.BeaconNode.Tests.Configuration
{
    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void JsonDevelopmentConfig()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json")
                .Build();
            services.AddBeaconNode(configuration);
            var testServiceProvider = services.BuildServiceProvider();

            // Act
            var miscellaneousParameters = testServiceProvider.GetService<IOptions<MiscellaneousParameters>>().Value;
            var gweiValues = testServiceProvider.GetService<IOptions<GweiValues>>().Value;
            var initialValues = testServiceProvider.GetService<IOptions<InitialValues>>().Value;
            var timeParameters = testServiceProvider.GetService<IOptions<TimeParameters>>().Value;
            var stateListLengths = testServiceProvider.GetService<IOptions<StateListLengths>>().Value;
            var rewardsAndPenalties = testServiceProvider.GetService<IOptions<RewardsAndPenalties>>().Value;
            var maxOperationsPerBlock = testServiceProvider.GetService<IOptions<MaxOperationsPerBlock>>().Value;
            var signatureDomains = testServiceProvider.GetService<IOptions<SignatureDomains>>().Value;

            // Assert
            miscellaneousParameters.ChurnLimitQuotient.ShouldNotBe(0uL);
            miscellaneousParameters.MaximumCommitteesPerSlot.ShouldNotBe(0uL);
            miscellaneousParameters.MaximumValidatorsPerCommittee.ShouldNotBe(0uL);
            miscellaneousParameters.MinimumGenesisActiveValidatorCount.ShouldNotBe(0);
            miscellaneousParameters.MinimumGenesisTime.ShouldNotBe(0uL);
            miscellaneousParameters.MinimumPerEpochChurnLimit.ShouldNotBe(0uL);
            miscellaneousParameters.ShuffleRoundCount.ShouldNotBe(0);
            miscellaneousParameters.TargetCommitteeSize.ShouldNotBe(0uL);

            gweiValues.EffectiveBalanceIncrement.ShouldNotBe(Gwei.Zero);
            gweiValues.EjectionBalance.ShouldNotBe(Gwei.Zero);
            gweiValues.MaximumEffectiveBalance.ShouldNotBe(Gwei.Zero);

            // both actually should be zero
            initialValues.BlsWithdrawalPrefix.ShouldBe((byte)0);
            initialValues.GenesisEpoch.ShouldBe(Epoch.Zero);

            timeParameters.MaximumSeedLookahead.ShouldNotBe(Epoch.Zero);
            timeParameters.MinimumAttestationInclusionDelay.ShouldNotBe(Slot.Zero);
            timeParameters.MinimumEpochsToInactivityPenalty.ShouldNotBe(Epoch.Zero);
            timeParameters.MinimumSeedLookahead.ShouldNotBe(Epoch.Zero);
            timeParameters.MinimumValidatorWithdrawabilityDelay.ShouldNotBe(Epoch.Zero);
            timeParameters.PersistentCommitteePeriod.ShouldNotBe(Epoch.Zero);
            timeParameters.SecondsPerSlot.ShouldNotBe(0uL);
            timeParameters.SlotsPerEpoch.ShouldNotBe(Slot.Zero);
            timeParameters.SlotsPerEth1VotingPeriod.ShouldNotBe(Slot.Zero);
            timeParameters.SlotsPerHistoricalRoot.ShouldNotBe(Slot.Zero);

            stateListLengths.EpochsPerHistoricalVector.ShouldNotBe(Epoch.Zero);
            stateListLengths.EpochsPerSlashingsVector.ShouldNotBe(Epoch.Zero);
            stateListLengths.HistoricalRootsLimit.ShouldNotBe(0uL);
            stateListLengths.ValidatorRegistryLimit.ShouldNotBe(0uL);

            rewardsAndPenalties.BaseRewardFactor.ShouldNotBe(0uL);
            rewardsAndPenalties.InactivityPenaltyQuotient.ShouldNotBe(0uL);
            rewardsAndPenalties.MinimumSlashingPenaltyQuotient.ShouldNotBe(0uL);
            rewardsAndPenalties.ProposerRewardQuotient.ShouldNotBe(0uL);
            rewardsAndPenalties.WhistleblowerRewardQuotient.ShouldNotBe(0uL);

            maxOperationsPerBlock.MaximumAttestations.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumAttesterSlashings.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumDeposits.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumProposerSlashings.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumVoluntaryExits.ShouldNotBe(0uL);

            // actually should be zero
            signatureDomains.BeaconProposer.ShouldBe(default);

            signatureDomains.BeaconAttester.ShouldNotBe(default);
            signatureDomains.Deposit.ShouldNotBe(default);
            signatureDomains.Randao.ShouldNotBe(default);
            signatureDomains.VoluntaryExit.ShouldNotBe(default);
        }

        [TestMethod]
        public void YamlMinimalConfig()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
            var configuration = new ConfigurationBuilder()
                .AddYamlFile("minimal.yaml")
                .Build();
            services.AddBeaconNode(configuration);
            var testServiceProvider = services.BuildServiceProvider();

            // Act
            var miscellaneousParameters = testServiceProvider.GetService<IOptions<MiscellaneousParameters>>().Value;
            var gweiValues = testServiceProvider.GetService<IOptions<GweiValues>>().Value;
            var initialValues = testServiceProvider.GetService<IOptions<InitialValues>>().Value;
            var timeParameters = testServiceProvider.GetService<IOptions<TimeParameters>>().Value;
            var stateListLengths = testServiceProvider.GetService<IOptions<StateListLengths>>().Value;
            var rewardsAndPenalties = testServiceProvider.GetService<IOptions<RewardsAndPenalties>>().Value;
            var maxOperationsPerBlock = testServiceProvider.GetService<IOptions<MaxOperationsPerBlock>>().Value;
            var signatureDomains = testServiceProvider.GetService<IOptions<SignatureDomains>>().Value;

            // Assert
            miscellaneousParameters.ChurnLimitQuotient.ShouldNotBe(0uL);
            miscellaneousParameters.MaximumCommitteesPerSlot.ShouldNotBe(0uL);
            miscellaneousParameters.MaximumValidatorsPerCommittee.ShouldNotBe(0uL);
            miscellaneousParameters.MinimumGenesisActiveValidatorCount.ShouldNotBe(0);
            miscellaneousParameters.MinimumGenesisTime.ShouldNotBe(0uL);
            miscellaneousParameters.MinimumPerEpochChurnLimit.ShouldNotBe(0uL);
            miscellaneousParameters.ShuffleRoundCount.ShouldNotBe(0);
            miscellaneousParameters.TargetCommitteeSize.ShouldNotBe(0uL);

            gweiValues.EffectiveBalanceIncrement.ShouldNotBe(Gwei.Zero);
            gweiValues.EjectionBalance.ShouldNotBe(Gwei.Zero);
            gweiValues.MaximumEffectiveBalance.ShouldNotBe(Gwei.Zero);

            // both actually should be zero
            initialValues.BlsWithdrawalPrefix.ShouldBe((byte)0);
            initialValues.GenesisEpoch.ShouldBe(Epoch.Zero);

            timeParameters.MaximumSeedLookahead.ShouldNotBe(Epoch.Zero);
            timeParameters.MinimumAttestationInclusionDelay.ShouldNotBe(Slot.Zero);
            timeParameters.MinimumEpochsToInactivityPenalty.ShouldNotBe(Epoch.Zero);
            timeParameters.MinimumSeedLookahead.ShouldNotBe(Epoch.Zero);
            timeParameters.MinimumValidatorWithdrawabilityDelay.ShouldNotBe(Epoch.Zero);
            timeParameters.PersistentCommitteePeriod.ShouldNotBe(Epoch.Zero);
            timeParameters.SecondsPerSlot.ShouldNotBe(0uL);
            timeParameters.SlotsPerEpoch.ShouldNotBe(Slot.Zero);
            timeParameters.SlotsPerEth1VotingPeriod.ShouldNotBe(Slot.Zero);
            timeParameters.SlotsPerHistoricalRoot.ShouldNotBe(Slot.Zero);

            stateListLengths.EpochsPerHistoricalVector.ShouldNotBe(Epoch.Zero);
            stateListLengths.EpochsPerSlashingsVector.ShouldNotBe(Epoch.Zero);
            stateListLengths.HistoricalRootsLimit.ShouldNotBe(0uL);
            stateListLengths.ValidatorRegistryLimit.ShouldNotBe(0uL);

            rewardsAndPenalties.BaseRewardFactor.ShouldNotBe(0uL);
            rewardsAndPenalties.InactivityPenaltyQuotient.ShouldNotBe(0uL);
            rewardsAndPenalties.MinimumSlashingPenaltyQuotient.ShouldNotBe(0uL);
            rewardsAndPenalties.ProposerRewardQuotient.ShouldNotBe(0uL);
            rewardsAndPenalties.WhistleblowerRewardQuotient.ShouldNotBe(0uL);

            maxOperationsPerBlock.MaximumAttestations.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumAttesterSlashings.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumDeposits.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumProposerSlashings.ShouldNotBe(0uL);
            maxOperationsPerBlock.MaximumVoluntaryExits.ShouldNotBe(0uL);

            // actually should be zero
            signatureDomains.BeaconProposer.ShouldBe(default);

            signatureDomains.BeaconAttester.ShouldNotBe(default);
            signatureDomains.Deposit.ShouldNotBe(default);
            signatureDomains.Randao.ShouldNotBe(default);
            signatureDomains.VoluntaryExit.ShouldNotBe(default);
        }

        [TestMethod]
        public void BothWithOverride() 
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddLogging(configure => configure.AddConsole());
            var configuration = new ConfigurationBuilder()
                .AddYamlFile("testconfigA.yaml")
                .AddJsonFile("testappsettingsB.json")
                .Build();
            services.AddBeaconNode(configuration);
            var testServiceProvider = services.BuildServiceProvider();

            // Act
            var miscellaneousParameters = testServiceProvider.GetService<IOptions<MiscellaneousParameters>>().Value;
            var gweiValues = testServiceProvider.GetService<IOptions<GweiValues>>().Value;
            var timeParameters = testServiceProvider.GetService<IOptions<TimeParameters>>().Value;

            // Assert

            // yaml only
            miscellaneousParameters.MaximumCommitteesPerSlot.ShouldBe(11uL);
            // json only
            miscellaneousParameters.TargetCommitteeSize.ShouldBe(22uL);
            // both yaml and json (jsonshould override)
            miscellaneousParameters.MaximumValidatorsPerCommittee.ShouldBe(23uL);
            // json only, different section
            gweiValues.MaximumEffectiveBalance.ShouldBe(new Gwei(24uL));
            // yaml only, no section in json
            timeParameters.SecondsPerSlot.ShouldBe(15uL);
        }
    }
}
