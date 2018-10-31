namespace Stateful.ServiceFabric.Internals
{
    using System.Runtime.Serialization;

    [DataContract(Name = "HashKeyNode_{0}", Namespace = "urn:Stateful/ServiceFabric/2018/10")]
    public class HashKeyNode<TKey>
    {
        [DataMember]
        public long? Previous { get; set; }

        [DataMember]
        public long? Next { get; set; }

        [DataMember]
        public TKey Key { get; set; }
    }
}