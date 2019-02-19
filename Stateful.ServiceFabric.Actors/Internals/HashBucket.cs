namespace Stateful.ServiceFabric.Actors.Internals
{
    using System.Runtime.Serialization;

    [DataContract(Namespace = "urn:Stateful/ServiceFabric/2018/10")]
    public class HashBucket
    {
        [DataMember]
        public long HashCode { get; set; }

        [DataMember]
        public long? Previous { get; set; }

        [DataMember]
        public long? Next { get; set; }

        [DataMember]
        public long? Head { get; set; }

        [DataMember]
        public long? Tail { get; set; }
    }
}