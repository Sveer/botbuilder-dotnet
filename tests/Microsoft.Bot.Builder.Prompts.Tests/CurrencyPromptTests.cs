﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Middleware;
using Microsoft.Bot.Builder.Storage;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Currency Prompts")]
    public class CurrencyPromptTests
    {
        [TestMethod]
        public async Task CurrencyPrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new CurrencyPrompt(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var currencyResult = await testPrompt.Recognize(context);
                        if (currencyResult.Succeeded())
                        {
                            Assert.IsTrue(currencyResult.Value != float.NaN);
                            Assert.IsNotNull(currencyResult.Text);
                            Assert.IsNotNull(currencyResult.Unit);
                            Assert.IsInstanceOfType(currencyResult.Value, typeof(float));
                            context.Reply($"{currencyResult.Value} {currencyResult.Unit}");
                        }
                        else
                            context.Reply(currencyResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
                .Send(" I would like $45.50")
                    .AssertReply("45.5 Dollar")
                .StartTest();
        }

        [TestMethod]
        public async Task CurrencyPrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var numberPrompt = new CurrencyPrompt(Culture.English, async (ctx, result) =>
                {
                    if (result.Value <= 10)
                        result.Status = RecognitionStatus.TooSmall;
                });

                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await numberPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var currencyPrompt = await numberPrompt.Recognize(context);
                    if (currencyPrompt.Succeeded())
                        context.Reply($"{currencyPrompt.Value} {currencyPrompt.Unit}");
                    else
                        context.Reply(currencyPrompt.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send(" I would like $1.00")
                    .AssertReply(RecognitionStatus.TooSmall.ToString())
                .Send(" I would like $45.50")
                    .AssertReply("45.5 Dollar")
                .StartTest();
        }

    }
}