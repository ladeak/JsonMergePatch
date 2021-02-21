using LaDeak.JsonMergePatch.Abstractions;

namespace LaDeak.JsonMergePatch.AspNetCore.Tests
{
    public class TestDtoWrapped : Patch<TestDto>
    {
        public TestDtoWrapped()
        {
            Properties = new bool[1];
        }

        private int? _prop1;
        public int? Prop1 { get => _prop1; set { Properties[0] = true; _prop1 = value; } }

        public override TestDto ApplyPatch(TestDto input)
        {
            input ??= new();
            if (Properties[0])
                input.Prop1 = Prop1.HasValue ? Prop1.Value : default;
            return input;
        }
    }
}
