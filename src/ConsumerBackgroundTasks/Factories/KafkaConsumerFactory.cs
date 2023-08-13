using System;
using System.Collections;
using Confluent.Kafka;

namespace ConsumerBackgroundTasks.Factories
{
	public class KafkaConsumerFactory : IKafkaConsumerFactory
	{
		public KafkaConsumerFactory()
		{
		}

        public IConsumer<T, U> CreateKafkaConsumer<T, U>(IEnumerable<KeyValuePair<string, string>> config)
        {
            return new ConsumerBuilder<T, U>(config).Build();
        }
    }
}

