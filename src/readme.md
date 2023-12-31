﻿- [Kafka notes](#kafka-notes)
- [1. Moving pieces](#1-moving-pieces)
  * [1.1 Message](#11-message)
  * [1.2 Batch](#12-batch)
  * [1.3 Topic](#13-topic)
- [2. Components](#2-components)
  * [2.1 Producer](#21-producer)
  * [2.2 Broker](#22-broker)
  * [2.3 Consumer](#23-consumer)
  * [2.4 Consumer groups](#24-consumer-groups)
- [3. Apis](#3-apis)
  * [3.1 Producer](#31-producer)
  * [3.2 Consumer](#32-consumer)
- [4. Design decisions](#4-design-decisions)
  * [4.1 Simple storage](#41-simple-storage)
  * [4.2 Message Id or Offsets](#42-message-id-or-offsets)
  * [4.3 Batch](#43-batch)
  * [4.4 Caching](#44-caching)
  * [4.5 Broker stateless](#45-broker-stateless)
  * [4.6 Message deletion](#46-message-deletion)
  * [4.7 Side effect](#47-side-effect)
- [5. Distributed coordination](#5-distributed-coordination)
  * [5.1 Publishing](#51-publishing)
  * [5.2 Consumer group](#52-consumer-group)
  * [5.3 Zookeeper](#53-zookeeper)
    + [5.3.1 Zookeeper registries](#531-zookeeper-registries)
  * [5.4 Ephemeral and Persistent node](#54-ephemeral-and-persistent-node)
  * [5.5 Zookeeper watcher](#55-zookeeper-watcher)
  * [5.6 Rebalance process](#56-rebalance-process)
- [6. Delivery guarantees](#6-delivery-guarantees)
  * [6.1 Atleast once](#61-atleast-once)
  * [6.2 Exactly once](#62-exactly-once)
  * [6.3 In order delivery](#63-in-order-delivery)
  * [6.4 Replication](#64-replication)
- [7. Cheatsheet](#7-cheatsheet)
  * [7.1 Access broker cli](#71-access-broker-cli)
  * [7.2 topic](#72-topic)
  * [7.3 producer test](#73-producer-test)
  * [7.4 consume test](#74-consume-test)
  * [7.5 consumer group info](#75-consumer-group-info)
  * [7.5 create a consumer and join a consumer group to consume](#75-create-a-consumer-and-join-a-consumer-group-to-consume)

<small><i><a href='http://ecotrust-canada.github.io/markdown-toc/'>Table of contents generated with markdown-toc</a></i></small>


# Kafka notes

# 1. Moving pieces
## 1.1 Message
* Basic unit of work
* Contain payload of bytes
* (Optional) key metadata payload, can be hashed to write messages to specific partitions in a topic to ensure messages of same key are in same parititio

## 1.2 Batch
* Messages are exported in batches. A batch is a set of messages that exist in the same partition of a topic.
* This is for throughput and removes need for each message to require full TCP roundtrip.

## 1.3 Topic
* Topics are like a stream that moves from producer to consumer
* Topics are comprised of partitions
* Messages can only be appended to partitions of a topic, which are read from beginning to end.
* Messages in single partitions are read in order. No guarantee across different partitions.
* Topic provide scalability due to partitions can be saved in different servers.
```
       Topic
---------------
Partition 1
[m1][m2][m3]
---------------
Partition 2
[m1][m2]
---------------
```

# 2. Components
## 2.1 Producer
* Publishes to broker in form of topics
* Can publish multiple messages in a batch
* Contains
a) Producer record
* An event is called a message / record
* Composed of headers (optional metadata), key, value, timestamp (default is created if not provided)
b) Serializer
* First thing producer does on a message is convert the key and value into byte array
c) Partitioner
* After serialization message goes into partitioner, which returns the partitions of a specific topic the message is assigned.
* If key is specified in producer record, hash function is used on key and mapped to a partition
* If a partition is specified in the producer record, partitioner is skipped, message is sent to the partition specified
By default one partition for each topic (each partition get its own id)
* Producer keeps track of destination after partitioner provides the destination, and it saves it in the buffer to be sent in batches.

## 2.2 Broker
* Set of servers. (individual server is a broker)
* Stores messages produced
* When broker receives the message, it
a) Assigns an offset to the message
b) Stores on disk
c) sends a response back to the producer
* If successfully written, metadata of record including topic, partition, and offset
* If failed, sends back error to the producer, which will retry a few times before returning an error.
e) Broker can handle millions of record as long as it's specced for it.
f) Clustered. Zookeeper elects a one server in cluster as cluster controller that
* Assigns parititions to brokers
* Elects partition leaders
* Monitor broker failures
g) Benefits of multiple server/broker in a cluster
* Scalability
* Replication (Only 1 broker has the partition that talks to producer and consumer known as leader. If this leader fails, another is created)
Standard for HA is 3 replicas.
TLDR: Partitions are for throughput. Replication is for resiliency to server failure.

## 2.3 Consumer
* Subscribe to set of messages in a broker
* Consume subscribed set
a) Subscribe to one or more topics
b) Send pull request of subscribed topics to the brokers, which has information on data location, and it responds with records it has on disk
c) Read message from the partitions of the topic in order they are written
d) Keep track of messages they have already written consumed through offset
Offset is a unique int attached to every record in a partition as meta data incremented every time new message is published.
e) Produce and Pull request should be sent to partition leader (single partition residing on only 1 broker. If wrong broker gets request, it sends not leader error)
f) To figure out the leader in e, Producer and Consumer send metadata request to the brokers, including list of interested topic which responds with
list of partitions in the topic, replica partitions, leader partition. (Can be sent to any broker since they cache this metadata, which in turn caches locally on producer and consumer)

## 2.4 Consumer groups
Multiple consumers operate in groups
Enables horizontal scaling, but there are rules below on how they work
* Topic is consumed by one or more consumers working in a consumer group
* Ensures each partition is consumed only by a single member. So a member can consume messages from multiple parititons, but one partition will only have one consumer
* A partition being consumed by a member comes under the ownership of that particular member.

```            
partition 1 [1][2][3] ---> Consumer 1 (consumer group 1)
partition 2 [1][2] ----> Consumer 2 (consumer group 1)
partition 3 [1][2][3] ---> Consumer 2 (consumer group 1)
partition 4 [1][2][3][4] ---> Consumer 3 (consumer group 1)
```
# 3. Apis
https://docs.confluent.io/kafka-clients/dotnet/current/overview.html

use nuget:
Confluent.Kafka package,
optional:
Confluent.SchemaRegistry, Confluent.SchemaRegistry.Serdes.Protobuf, Confluent.SchemaRegistry.Serdes.Json and Confluent.SchemaRegistry.Serdes.Avro

## 3.1 Producer

## 3.2 Consumer
* Create one or more message streams for a topic by subscribing to it
* Each message stream has an iterator
* Consumer use iterator (Not conventional. Does not terminate when there are no more messages to consume) Over all messages of the continuous stream
* Iterators stops iterating until new records are published
* Support Point to Point and Pub Sub delivery

# 4. Design decisions
## 4.1 Simple storage
* Implemented like a logical log, broker appends records to the last segment. It is flushed to disk after segment gather certain amount of data or time.
* Consumer can only consume records that have been flushed to disk
* If a consumer fails before segment file is flushed to disk, all segment files and partitions saved in it will be inaccessible to consumer, and zookeeper elects a new leader

## 4.2 Message Id or Offsets
Kafka address each record by a logical offset, eliminating random access. Offset in partition is in increasing order, but it is not a constant increase due to varying message/record size
* to calculate new offset, size + prev message offset. Default max size of message is (1 mb)
* Consumer sends pull request to broker which keeps data buffer ready, it consumes messages from specific partition in sequence. A consumer has a specific offset to be tracked.
* Pull request is composed of an [offset number, and number of bytes]
* Users can set a limit on number of bytes a consumer can get from broker. (eg 1 mb). If a consumer tries to get a message > 1mb, it will error. Otherwise it will return messages with stacksize >= limit.

## 4.3 Batch
* Producer can dispatch in batches in each send request
* Consumer can get in batches even though it goes through each message in seq

## 4.4 Caching
* Relys on file's paging for 'caching'

## 4.5 Broker stateless
* Consumption info is not kept on broker. it is kept on zookeeper.

## 4.6 Message deletion
Kafka maintains time-based SLA for keeping data. byte or days, whichever comes first if both are set.

## 4.7 Side effect
Consumers can reconsume previously consumed messages by rewinding offset
* When a error occurs at consumer, they can rewind and consume after dealing with the error for safe consumption
* When data is flushed to a persistent store, and a consumer fails, we no longer have a track of the unflushed data, it can be catered by checkpointing offsets after a certain gap and start from last checkpointed offset.
Duplicates may occur from above method, but checkpoint frequency reduces amount of duplicates.

# 5. Distributed coordination
## 5.1 Publishing
3 ways
1) Publish to specific partition
2) Publish to random partition
3) Publish to partition based on key

## 5.2 Consumer group
* Partitions are smallest unit of parallelism
* Each parititon can only be consumed by a single consumer (this design decision avoids needed coordination between consumers)
* If one consumer dies, all other consumers need to rebalance the work. To balance load with fewer consumers, over partition a topic and assigning multiple partitions to a consumer
* Consumers coordinate with each other via zookeeper as a no managed node (decentralized decision maker)

## 5.3 Zookeeper
* Detect addition or removal of consumer and brokers
* Register consumer and broker by creating ephemeral nodes
all brokers try to create ephermeral node called broker. only first broker that starts in a cluster will exceed, others will error with "node already exist"
* When broker fails, its node is removed. If the controller is lost, then all brokers are notified and they elect a new controller. First to do so is controller.
* Initiate a load rebalance process in case broker or consumers are added or removed
* Maintain the consumption from brokers and track the offset of the last consumed messages of a partition.

### 5.3.1 Zookeeper registries
* Consumer registry
saves consumer info such as which consumer group they below to and which topic is subscribed
* Broker registry
saves broker info, host, port, and the partitions the broker has
* Ownership registry
saves info on subscribed partitions, such as which consumer id is consuming message from which partition.
* Offset registry
saves info on offsets per partition.
new consumer groups has no offset and they must start from smallest or largest offset in partition

## 5.4 Ephemeral and Persistent node
* Ephemeral - nodes in broker, consumer, ownership registry
Gets removed when their creator is down.
In case of broker failure, all of its partitions saved in it are down, resulting in loss of info from the broker registry
In case of consumer failure, the info nodes it had in the consumer registry is removed as well as info regarding partitions it owned in the ownership registry
* Persistent - nodes in offset

## 5.5 Zookeeper watcher
Broker and consumer registry can have watchers registered on them by each consumer and broker.
* watcher notifies change
* a change in broker registry occurs when a topic that a consumer is consumed is modified
* a change in consumer registry occurs when a consumer fails or a new consumer is added
watcher also trigger a load rebalance whenever consumer or broker change occurs, or when a new consumer is initialized

## 5.6 Rebalance process
* stops consumption during the process until it is done
* causes consumers who redefined their ownership refresh their cache which slows down consumptions more

# 6. Delivery guarantees
## 6.1 Atleast once
Default is atleast once
## 6.2 Exactly once
This needs code logic in app. Some techniques below
* use unique id
* app can track consumed record with its offset in a transaction together. (if supported)
## 6.3 In order delivery
messages in partition in a specific order will be consumed in same order
## 6.4 Replication
Kafka can replicate partition multiple times and designate one as leader. producer and consumer can only do it to the partition leader.
All other partitions need to sync with leader replica so if leader is lost they can re-elect a new one.
There are in sync replica (recent configured heartbeat received, recent message synced from leader) and out of sync replicas. kafka can only pick in sync replica as leader while rest syncs.

# 7. Cheatsheet
## 7.1 Access broker cli
get in kafka broker. and treat itself as the bootstrap server (for getting metadata). Go to bin directory where executables are
``` 
docker exec -it broker bash
cd /bin
```

## 7.2 topic
creating a new topic
```
kafka-topics --create --bootstrap-server localhost:9092 --replication-factor 1 --partitions 1 --topic test
```
check created topic
```
kafka-topics --bootstrap-server=localhost:9092 --list
```

## 7.3 producer test
Open a new cli, go to broker bash /bin
```
kafka-console-producer --bootstrap-server localhost:9092 --topic test
start producing messages
```

## 7.4 consume test
open a new cli, go to broker bash /bin
```
kafka-console-consumer --bootstrap-server localhost:9092 --topic test --from-beginning
```

exiting here will cause the process to commit the offset. notice it was ok to not commit because one partition can only be consumed by 1 consumer anyway so there isn't anything it needs to coordinate with another consumer with.

## 7.5 consumer group info
get the consumer group. by default new consumers without specifying a consumer group will have auto created a consumer group
```
kafka-consumer-groups --bootstrap-server localhost:9092 --list
```
describe it
```
kafka-consumer-groups --bootstrap-server localhost:9092 --group CONSUMERGROUPFROMABOVE --describe
```

## 7.5 create a consumer and join a consumer group to consume
Join group and start where offset was left off
```
kafka-console-consumer --bootstrap-server localhost:9092 --topic test --group consumer-group-98828
```
can start from beginning as well
```
kafka-console-consumer --bootstrap-server localhost:9092 --topic test --group consumer-group-98828 --from-beginning
```
start from a specific offset. Notice "Options group and partition cannot be specified together.""
```
kafka-console-consumer --bootstrap-server localhost:9092 --topic test --property print.key=true --property key.separator="-" --partition 0 --offset 3
```