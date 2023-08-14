using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace ConsumerTest;

public class TPLTests
{
    private readonly ITestOutputHelper _output;

    public TPLTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async void TestPost()
    {
        // we can see with this test that once the Block buffer BoundedCapacity is full, posting will return false and skip it.
        var processorCount = Environment.ProcessorCount;
        _output.WriteLine($"Testing actionblock.Post with Processor count {processorCount}");
        var actionBlock = new ActionBlock<int>(value =>
        {
            _output.WriteLine($"Processed {value} on processor id: {Thread.GetCurrentProcessorId()}");       
        },
        new ExecutionDataflowBlockOptions
        {
            // 3 threads to process in parallel
            MaxDegreeOfParallelism = 3,
            BoundedCapacity = 3
        });

        int maxPost = 10000;
        int numOfAccepted = 0;
        for (int i = 0; i < maxPost; i++)
        {
            _output.WriteLine($"Posting {i}");
            var accepted = actionBlock.Post(i);
            _output.WriteLine($"Accepted posting? {accepted}");
            numOfAccepted += accepted ? 1 : 0;
        }

        _output.WriteLine($"Posted {maxPost} times");
        _output.WriteLine($"Processor count {processorCount}");

        actionBlock.Complete();
        await actionBlock.Completion.WaitAsync(TimeSpan.FromSeconds(10));
        _output.WriteLine("Test completed");
        Assert.True(maxPost > numOfAccepted);
    }

    [Fact]
    public async void TestSendAsync()
    {
        // We can see with this test that once posting buffer is full, send async will block until the buffer has space again thus resulting in
        // every message getting handled eventually.
        var processorCount = Environment.ProcessorCount;
        _output.WriteLine($"Testing actionblock.Post with Processor count {processorCount}");
        var actionBlock = new ActionBlock<int>(value =>
        {
            _output.WriteLine($"Processed {value} on processor id: {Thread.GetCurrentProcessorId()}");
        },
        new ExecutionDataflowBlockOptions
        {
            // 3 threads to process in parallel
            MaxDegreeOfParallelism = 3,
            BoundedCapacity = 3
        });

        int maxPost = 100;
        int numOfAccepted = 0;
        for (int i = 0; i < maxPost; i++)
        {
            _output.WriteLine($"Posting {i}");
            var accepted = await actionBlock.SendAsync(i);
            _output.WriteLine($"Accepted posting? {accepted}");
            numOfAccepted += accepted ? 1 : 0;
        }

        _output.WriteLine($"Posted {maxPost} times");
        _output.WriteLine($"Processor count {processorCount}");

        actionBlock.Complete();
        await actionBlock.Completion.WaitAsync(TimeSpan.FromSeconds(10));
        _output.WriteLine("Test completed");

        Assert.Equal(maxPost, numOfAccepted);
    }

    [Fact]
    public async void TestBufferReduceWhenActionBlockProcessAndNotAfterFinish()
    {
        // Test to see if the buffer is considered dequeued when a message is sent down the processing method or when a message is finished processed
        // 1) sent to processing method dequeues, we should posting up to 6 before a process completes.
        // 2) dequeued only after processing method finished. we should seeOnly accept 3 at a time. accepting 4th one when one of the block finishes processing in 5 seconds.
        // Conclusion: from the results below, we can conclude 2) is true. Only after completion of the action method will reduce the buffer in queue by one; sending it down to the method doesn't dequeue quite yet.
        var processorCount = Environment.ProcessorCount;
        _output.WriteLine($"Testing actionblock.Post with Processor count {processorCount}");
        var actionBlock = new ActionBlock<int>(value =>
        {
            Thread.Sleep(5000);
            _output.WriteLine($"Processed {value} on processor id: {Thread.GetCurrentProcessorId()}");
        },
        new ExecutionDataflowBlockOptions
        {
            // 3 threads to process in parallel
            MaxDegreeOfParallelism = 3,
            BoundedCapacity = 3
        });

        int maxPost = 10;
        int numOfAccepted = 0;
        for (int i = 0; i < maxPost; i++)
        {
            _output.WriteLine($"Posting {i}");
            var accepted = await actionBlock.SendAsync(i);
            _output.WriteLine($"Accepted posting? {accepted}");
            numOfAccepted += accepted ? 1 : 0;
        }

        _output.WriteLine($"Posted {maxPost} times");
        _output.WriteLine($"Processor count {processorCount}");

        actionBlock.Complete();
        await actionBlock.Completion.WaitAsync(TimeSpan.FromSeconds(10));
        _output.WriteLine("Test completed");
        Assert.Equal(maxPost, numOfAccepted);

        /* result
        Console output:
        Testing actionblock.Post with Processor count 8
        Posting 0
        Accepted posting? True
        Posting 1
        Accepted posting? True
        Posting 2
        Accepted posting? True
        Posting 3
        Processed 2 on processor id: 5
        Processed 1 on processor id: 7
        Processed 0 on processor id: 11
        Accepted posting? True
        Posting 4
        Accepted posting? True
        Posting 5
        Accepted posting? True
        Posting 6
        Processed 3 on processor id: 5
        Accepted posting? True
        Posting 7
        Processed 4 on processor id: 11
        Processed 5 on processor id: 7
        Accepted posting? True
        Posting 8
        Accepted posting? True
        Posting 9
        Processed 6 on processor id: 5
        Accepted posting? True
        Posted 10 times
        Processor count 8
        Processed 7 on processor id: 11
        Processed 8 on processor id: 7
        Processed 9 on processor id: 5
        Test completed
        */
}
}
