﻿using System;
using System.Threading.Tasks;
using Cortex.BeaconNode.Configuration;
using Cortex.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Cortex.BeaconNode.Tests
{
    [TestClass]
    public class BeaconChainTest
    {
        //[TestMethod]
        //public void InitialDefaultGenesisTimeShouldBeCorrect()
        //{
        //    // Arrange
        //    var beaconChain = new BeaconChain();

        //    // Act
        //    ulong time = beaconChain.State.GenesisTime;

        //    // Assert
        //    // TODO: Right now this doesn't do much, but will do some proper testing once initialiation is built
        //    var expectedInitialGenesisTime = (ulong)106185600;
        //    time.ShouldBe(expectedInitialGenesisTime);
        //}

        [TestMethod]
        public async Task GenesisWithEmptyParametersTimeShouldReject()
        {
            // Arrange
            var testServiceProvider = TestSystem.BuildTestServiceProvider();

            var chainConstants = testServiceProvider.GetService<ChainConstants>();
            var miscellaneousParameterOptions = testServiceProvider.GetService<IOptionsMonitor<MiscellaneousParameters>>();
            var gweiValueOptions = testServiceProvider.GetService<IOptionsMonitor<GweiValues>>();
            var initialValueOptions = testServiceProvider.GetService<IOptionsMonitor<InitialValues>>();
            var timeParameterOptions = testServiceProvider.GetService<IOptionsMonitor<TimeParameters>>();
            var stateListLengthOptions = testServiceProvider.GetService<IOptionsMonitor<StateListLengths>>();
            var rewardsAndPenaltiesOptions = testServiceProvider.GetService<IOptionsMonitor<RewardsAndPenalties>>();
            var maxOperationsPerBlockOptions = testServiceProvider.GetService<IOptionsMonitor<MaxOperationsPerBlock>>();
            var signatureDomainOptions = testServiceProvider.GetService<IOptionsMonitor<SignatureDomains>>();

            miscellaneousParameterOptions.CurrentValue.MinimumGenesisActiveValidatorCount = 2;

            var loggerFactory = new LoggerFactory(new[] {
                new ConsoleLoggerProvider(TestOptionsMonitor.Create(new ConsoleLoggerOptions()))
            });

            var cryptographyService = new CryptographyService();
            var beaconChainUtility = new BeaconChainUtility(loggerFactory.CreateLogger<BeaconChainUtility>(),
                miscellaneousParameterOptions, gweiValueOptions, timeParameterOptions,
                cryptographyService);
            var beaconStateAccessor = new BeaconStateAccessor(miscellaneousParameterOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, signatureDomainOptions,
                cryptographyService, beaconChainUtility);
            var beaconStateMutator = new BeaconStateMutator(chainConstants, timeParameterOptions, stateListLengthOptions, rewardsAndPenaltiesOptions,
                beaconChainUtility, beaconStateAccessor);
            var beaconStateTransition = new BeaconStateTransition(loggerFactory.CreateLogger<BeaconStateTransition>(),
                chainConstants, miscellaneousParameterOptions, gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, rewardsAndPenaltiesOptions, maxOperationsPerBlockOptions, signatureDomainOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor, beaconStateMutator);
            var beaconChain = new BeaconChain(loggerFactory.CreateLogger<BeaconChain>(),
                chainConstants, miscellaneousParameterOptions,
                gweiValueOptions, initialValueOptions, timeParameterOptions, stateListLengthOptions, maxOperationsPerBlockOptions,
                cryptographyService, beaconChainUtility, beaconStateAccessor, beaconStateMutator, beaconStateTransition);

            // Act
            var eth1BlockHash = Hash32.Zero;
            var eth1Timestamp = (ulong)106185600; // 1973-05-14
            var deposits = Array.Empty<Deposit>();
            var success = await beaconChain.TryGenesisAsync(eth1BlockHash, eth1Timestamp, deposits);

            // Assert
            success.ShouldBeFalse();
            beaconChain.State.ShouldBeNull();
        }
    }
}
