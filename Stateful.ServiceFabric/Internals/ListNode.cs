﻿namespace Stateful.ServiceFabric.Internals
{
    using System.Runtime.Serialization;

    [DataContract(Name = "ListNode_{0}", Namespace = "urn:Stateful/ServiceFabric/2018/10")]
    public class ListNode<T>
    {
        [DataMember]
        public long? Previous { get; set; }

        [DataMember]
        public long? Next { get; set; }

        [DataMember]
        public T Value { get; set; }
    }
}