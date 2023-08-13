using System;
using Confluent.Kafka;
namespace ConsumerBackgroundTasks.Factories
{
	public interface IKafkaConsumerFactory
	{
        /// <summary>
        /// Kafka consumer is not threadsafe. Return new instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="config"></param>
        /// <returns></returns>
		IConsumer<T, U> CreateKafkaConsumer<T, U>(IEnumerable<KeyValuePair<string, string>> config);
	}
}

