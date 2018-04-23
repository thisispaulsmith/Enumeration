using NetSmith.Enumeration;

namespace UnitTests
{
    public abstract class TestEnumeration : Enumeration
    {
        public static readonly TestEnumeration Test1 = new TestEnumeration1(0, "Test1");
        public static readonly TestEnumeration Test2 = new TestEnumeration2(1, "Test2");
        public static readonly TestEnumeration Test3 = new TestEnumeration3(2, "Test3");

        protected TestEnumeration(byte value, string displayName)
            : base(value, displayName)
        { }

        private class TestEnumeration1 : TestEnumeration
        {
            public TestEnumeration1(byte value, string displayName)
                    : base(value, displayName)
            { }
        }

        private class TestEnumeration2 : TestEnumeration
        {
            public TestEnumeration2(byte value, string displayName)
                    : base(value, displayName)
            { }
        }

        private class TestEnumeration3 : TestEnumeration
        {
            public TestEnumeration3(byte value, string displayName)
                    : base(value, displayName)
            { }
        }
    }
}
