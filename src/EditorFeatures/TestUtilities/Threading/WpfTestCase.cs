// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Utilities;
using Xunit.Abstractions;
using Xunit.Sdk;
using Roslyn.Utilities;
using Xunit;
using System.Reflection;
using System.Collections.Generic;

namespace Roslyn.Test.Utilities
{
    public class WpfTestCase : XunitTestCase
    {
        private readonly SemaphoreSlim _wpfTestSerializationGate;

        public WpfTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay defaultMethodDisplay, ITestMethod testMethod, SemaphoreSlim wpfTestSerializationGate, object[] testMethodArguments = null)
            : base(diagnosticMessageSink, defaultMethodDisplay, testMethod, testMethodArguments)
        {
            _wpfTestSerializationGate = wpfTestSerializationGate;
        }

        public override Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink, IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            var sta = StaTaskScheduler.DefaultSta;
            var task = Task.Factory.StartNew(async () =>
            {
                Debug.Assert(sta.Threads.Length == 1);
                Debug.Assert(sta.Threads[0] == Thread.CurrentThread);

                using (await _wpfTestSerializationGate.DisposableWaitAsync())
                {
                    try
                    {
                        // Sync up FTAO to the context that we are creating here. 
                        ForegroundThreadAffinitizedObject.CurrentForegroundThreadData = new ForegroundThreadData(
                            Thread.CurrentThread,
                            StaTaskScheduler.DefaultSta,
                            ForegroundThreadDataKind.StaUnitTest);

                        // Reset our flag ensuring that part of this test actually needs WpfFact
                        s_wpfFactRequirementReason = null;

                        // All WPF Tests need a DispatcherSynchronizationContext and we dont want to block pending keyboard
                        // or mouse input from the user. So use background priority which is a single level below user input.
                        var dispatcherSynchronizationContext = new DispatcherSynchronizationContext();

                        // xUnit creates its own synchronization context and wraps any existing context so that messages are
                        // still pumped as necessary. So we are safe setting it here, where we are not safe setting it in test.
                        SynchronizationContext.SetSynchronizationContext(dispatcherSynchronizationContext);

                        // Just call back into the normal xUnit dispatch process now that we are on an STA Thread with no synchronization context.
                        var baseTask = new WpfTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, TestMethodArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();

                        do
                        {
                            var delay = Task.Delay(TimeSpan.FromMilliseconds(10), cancellationTokenSource.Token);
                            var completed = await Task.WhenAny(baseTask, delay).ConfigureAwait(false);
                            if (completed == baseTask)
                            {
                                return await baseTask.ConfigureAwait(false);
                            }

                            // Schedule a task to pump messages on the UI thread.  
                            await Task.Factory.StartNew(
                                () => WaitHelper.WaitForDispatchedOperationsToComplete(DispatcherPriority.ApplicationIdle),
                                cancellationTokenSource.Token,
                                TaskCreationOptions.None,
                                sta).ConfigureAwait(false);
                        }
                        while (true);
                    }
                    finally
                    {
                        ForegroundThreadAffinitizedObject.CurrentForegroundThreadData = null;
                        s_wpfFactRequirementReason = null;

                        // Cleanup the synchronization context even if the test is failing exceptionally
                        SynchronizationContext.SetSynchronizationContext(null);
                    }
                }
            }, cancellationTokenSource.Token, TaskCreationOptions.None, sta);

            return task.Unwrap();
        }

        private static string s_wpfFactRequirementReason;

        /// <summary>
        /// Asserts that the test is running on a <see cref="WpfFactAttribute"/> test method, and records the reason for requiring the <see cref="WpfFactAttribute"/>.
        /// </summary>
        public static void RequireWpfFact(string reason)
        {
            if (ForegroundThreadDataInfo.CurrentForegroundThreadDataKind != ForegroundThreadDataKind.StaUnitTest)
            {
                throw new Exception($"This test requires {nameof(WpfFactAttribute)} because '{reason}' but is missing {nameof(WpfFactAttribute)}. Either the attribute should be changed, or the reason it needs an STA thread audited.");
            }

            s_wpfFactRequirementReason = reason;
        }

        private sealed class WpfTestCaseRunner : XunitTestCaseRunner
        {
            public WpfTestCaseRunner(IXunitTestCase testCase, string displayName, string skipReason, object[] constructorArguments, object[] testMethodArguments, IMessageBus messageBus, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
                : base(testCase, displayName, skipReason, constructorArguments, testMethodArguments, messageBus, aggregator, cancellationTokenSource)
            {
            }

            protected override Task<RunSummary> RunTestAsync()
            {
                return new WpfTestRunner(new XunitTest(TestCase, DisplayName), MessageBus, TestClass, ConstructorArguments, TestMethod, TestMethodArguments, SkipReason, BeforeAfterAttributes, new ExceptionAggregator(Aggregator), CancellationTokenSource).RunAsync();
            }
        }

        private sealed class WpfTestRunner : XunitTestRunner
        {
            public WpfTestRunner(ITest test, IMessageBus messageBus, Type testClass, object[] constructorArguments, MethodInfo testMethod, object[] testMethodArguments, string skipReason, IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
                : base(test, messageBus, testClass, constructorArguments, testMethod, testMethodArguments, skipReason, beforeAfterAttributes, aggregator, cancellationTokenSource)
            {
            }

            protected override async Task<decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
            {
                var runtime = await base.InvokeTestMethodAsync(aggregator);

                // Some part of the test should have asserted the need to use WpfFact. If none did, we should fail
                if (WpfTestCase.s_wpfFactRequirementReason == null)
                {
                    aggregator.Add(new Exception($"The test used {nameof(WpfFactAttribute)} but it does not require it. A call to {nameof(WpfTestCase)}.{nameof(RequireWpfFact)} should be added at the point where the requirement is needed."));
                }

                return runtime;
            }
        }
    }
}
