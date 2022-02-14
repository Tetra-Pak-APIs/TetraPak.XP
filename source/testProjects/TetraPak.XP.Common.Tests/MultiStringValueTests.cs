using Xunit;

namespace TetraPak.XP.Common.Tests
{
    public class MultiStringValueTests
    {
        [Fact]
        public void Test_initialization()
        {
            var @null = (MultiStringValue) null!;
            Assert.Null(@null);

            var empty = new MultiStringValue();
            Assert.True(empty.IsEmpty);
            
            var three = (MultiStringValue) "aaa,bbb,ccc";
            Assert.Equal(3, three.Count);
        }

        [Fact]
        public void Test_append_insert_and_removeAt()
        {
            var msv = (TestMultiStringValue) "aaa , bbb , ccc , ddd , eee";
            Assert.Equal(5, msv.Count);

            var test = msv.Removed(2, 2);
            Assert.Equal(3, test.Count);
            Assert.Equal("aaa, bbb, eee", test);

            test = test.Inserted(2, "ccc", "ddd");
            Assert.Equal("aaa, bbb, ccc, ddd, eee", test);
                
            test = msv.Appended("fff", "ggg");
            Assert.Equal("aaa, bbb, ccc, ddd, eee, fff, ggg", test);
        }
    }

    public class TestMultiStringValue : MultiStringValue
    {
        public static implicit operator TestMultiStringValue(string? stringValue) => new(stringValue);
        
        public TestMultiStringValue Appended(params string[] items)
        {
            var newItems = AddRange(items);
            return new TestMultiStringValue(newItems);
        }

        public TestMultiStringValue Removed(int index, int count = 1)
        {
            var newItems = RemoveAt(index, count);
            return new TestMultiStringValue(newItems);
        }

        public TestMultiStringValue Inserted(int index, params string[] items)
        {
            var newItems = InsertRange(index, items);
            return new TestMultiStringValue(newItems);
        }

        public TestMultiStringValue(string? stringValue) : base(stringValue)
        {
        }

        public TestMultiStringValue(string[] items) : base(items)
        {
        }
    }
}