using Gatekeeper.LdapPacketParserLibrary.Models.Operations;

namespace Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request
{
    public class ExtendedRequest : IProtocolOp
    {
        internal const int Tag = 23;

        public string RequestName { get; set; } = null!;
        public string? RequestValue { get; set; }

        static int GetTag()
        {
            return 23;
        }
    }
}
