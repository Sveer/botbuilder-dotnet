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
    [TestCategory("Range Prompts")]
    public class RangePromptTests
    {
        [TestMethod]
        public async Task RangePrompt_Test()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
                {
                    var state = ConversationState<TestState>.Get(context);
                    var testPrompt = new RangePrompt<int>(Culture.English);
                    if (!state.InPrompt)
                    {
                        state.InPrompt = true;
                        await testPrompt.Prompt(context, "Gimme:");
                    }
                    else
                    {
                        var rangeResult = await testPrompt.Recognize(context);
                        if (rangeResult.Succeeded())
                        {
                            Assert.IsTrue(rangeResult.Start > 0);
                            Assert.IsTrue(rangeResult.End > rangeResult.Start);
                            Assert.IsNotNull(rangeResult.Text);
                            context.Reply($"{rangeResult.Start}-{rangeResult.End}");
                        }
                        else
                            context.Reply(rangeResult.Status.ToString());
                    }
                })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("test test test")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
                .Send("give me 5 10")
                    .AssertReply(RecognitionStatus.NotRecognized.ToString())
                .Send(" give me between 5 and 10")
                    .AssertReply("5-10")
                .StartTest();
        }

        [TestMethod]
        public async Task RangePrompt_Validator()
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            await new TestFlow(adapter, async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new RangePrompt<int>(Culture.English, async (c, result) =>
                {
                    if (result.End - result.Start <= 5)
                        result.Status = RecognitionStatus.OutOfRange;
                });
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var rangeResult = await testPrompt.Recognize(context);
                    if (rangeResult.Succeeded())
                    {
                        Assert.IsTrue(rangeResult.Start > 0);
                        Assert.IsTrue(rangeResult.End > rangeResult.Start);
                        Assert.IsNotNull(rangeResult.Text);
                        context.Reply($"{rangeResult.Start}-{rangeResult.End}");
                    }
                    else
                        context.Reply(rangeResult.Status.ToString());
                }
            })
                .Send("hello")
                .AssertReply("Gimme:")
                .Send("give me between 1 and 4")
                    .AssertReply(RecognitionStatus.OutOfRange.ToString())
                .Send(" give me between 1 and 10")
                    .AssertReply("1-10")
                .StartTest();
        }

    }
}