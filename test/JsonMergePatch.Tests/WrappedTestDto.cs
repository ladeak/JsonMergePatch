using LaDeak.JsonMergePatch;

namespace LaDeak.JsonMergePatch.Tests
{
    public class WrappedTestDto : Patch<TestDto>
    {
        public int Prop1 { get; set; }

        public override TestDto ApplyPatch(TestDto input) => input;
    }
}
