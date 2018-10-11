namespace Stateful.ServiceFabric.Internals
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "urn:Stateful/ServiceFabric/2018/10")]
    public class ListManifest
    {
        [DataMember]
        public long Next { get; set; }

        [DataMember]
        public long Count { get; set; }

        [DataMember]
        public long? First { get; set; }

        [DataMember]
        public long? Last { get; set; }
    }
}