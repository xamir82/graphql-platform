using System;
using HotChocolate.Execution.Properties;
using Xunit;

namespace HotChocolate.Execution.Utilities
{
#nullable enable

    public class ObjectBufferTests
    {
        [Fact]
        public void Pop_PoolShouldCreateElement()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            PoolElement element = pool.Pop();

            // assert
            Assert.NotNull(element);
        }

        [Fact]
        public void TryPop_PoolShouldCreateElement()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            pool.TryPop(out PoolElement? element);

            // assert
            Assert.NotNull(element);
        }

        [Fact]
        public void Pop_PoolShouldThrowIfBufferIsFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            pool.Pop();
            pool.Pop();

            // assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => pool.Pop());

            Assert.Equal(Resources.ObjectBuffer_IsEmpty, exception.Message);
        }

        [Fact]
        public void TryPop_PoolShouldThrowIfBufferIsFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            pool.Pop();
            pool.Pop();
            var result = pool.TryPop(out PoolElement? poolElement);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void PopSafe_PoolShouldCreateElement()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            PoolElement element = pool.PopSafe();

            // assert
            Assert.NotNull(element);
        }

        [Fact]
        public void PopSafe_PoolShouldThrowIfBufferIsFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            pool.PopSafe();
            pool.PopSafe();

            // assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => pool.PopSafe());

            Assert.Equal(Resources.ObjectBuffer_IsEmpty, exception.Message);
        }

        [Fact]
        public void TryPopSafe_PoolShouldThrowIfBufferIsFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            pool.Pop();
            pool.Pop();
            var result = pool.TryPopSafe(out PoolElement? poolElement);

            // assert
            Assert.False(result);
        }

        [Fact]
        public void TryPopSafe_PoolShouldCreateElement()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });

            // act
            pool.TryPopSafe(out PoolElement? element);

            // assert
            Assert.NotNull(element);
        }

        [Fact]
        public void Push_PoolShouldTakeElementAndReturnTheSame()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });
            var element = new PoolElement();
            pool.Pop();

            // act
            pool.Push(element);

            // assert
            Assert.Equal(element, pool.Pop());
        }

        [Fact]
        public void Push_PoolShouldCallCleanOnReturn()
        {
            // arrange
            PoolElement? result = null;
            var pool = new ObjectBuffer<PoolElement>(2, x => { result = x; });
            var element = new PoolElement();
            pool.Pop();

            // act
            pool.Push(element);

            // assert
            Assert.Equal(element, result);
        }

        [Fact]
        public void TryPush_PoolShouldCallCleanOnReturn()
        {
            // arrange
            PoolElement? result = null;
            var pool = new ObjectBuffer<PoolElement>(2, x => { result = x; });
            var element = new PoolElement();
            pool.Pop();

            // act
            pool.TryPush(element);

            // assert
            Assert.Equal(element, result);
        }

        [Fact]
        public void Push_ThrowsIfFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });
            var element = new PoolElement();
            pool.Pop();
            pool.Pop();

            // act
            pool.Push(element);
            pool.Push(element);

            // assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => pool.Push(element));

            Assert.Equal(Resources.ObjectBuffer_IsUsedUp, exception.Message);
        }

        [Fact]
        public void TryPush_NotThrowsIfFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });
            var element = new PoolElement();
            pool.Pop();
            pool.Pop();

            // act
            pool.Push(element);
            pool.Push(element);
            var result = pool.TryPush(element);

            // assert 
            Assert.False(result);
        }

        [Fact]
        public void PushSafe_PoolShouldTakeElementAndReturnTheSame()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });
            var element = new PoolElement();
            pool.Pop();

            // act
            pool.PushSafe(element);

            // assert
            Assert.Equal(element, pool.Pop());
        }

        [Fact]
        public void PushSafe_ThrowsIfFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });
            var element = new PoolElement();
            pool.Pop();
            pool.Pop();

            // act
            pool.PushSafe(element);
            pool.PushSafe(element);

            // assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() => pool.PushSafe(element));

            Assert.Equal(Resources.ObjectBuffer_IsUsedUp, exception.Message);
        }

        [Fact]
        public void TryPushSafe_PoolShouldCallCleanOnReturn()
        {
            // arrange
            PoolElement? result = null;
            var pool = new ObjectBuffer<PoolElement>(2, x => { result = x; });
            var element = new PoolElement();
            pool.Pop();

            // act
            pool.TryPushSafe(element);

            // assert
            Assert.Equal(element, result);
        }


        [Fact]
        public void TryPushSafe_NotThrowsIfFull()
        {
            // arrange
            var pool = new ObjectBuffer<PoolElement>(2, x => { });
            var element = new PoolElement();
            pool.Pop();
            pool.Pop();

            // act
            pool.PushSafe(element);
            pool.PushSafe(element);
            var result = pool.TryPushSafe(element);

            // assert 
            Assert.False(result);
        }

        [Fact]
        public void PushSafe_PoolShouldCallCleanOnReturn()
        {
            // arrange
            PoolElement? result = null;
            var pool = new ObjectBuffer<PoolElement>(2, x => { result = x; });
            var element = new PoolElement();
            pool.Pop();

            // act
            pool.PushSafe(element);

            // assert
            Assert.Equal(element, result);
        }

        private class PoolElementCounter
        {
            public static int Counter = 0;

            public PoolElementCounter()
            {
                Counter++;
            }
        }

        private class PoolElement
        {
        }
    }
}
