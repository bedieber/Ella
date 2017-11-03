# Implementing a subscriber

Subscribers are pretty easy to implement. They do not interact with Ella until they first perform a subscribe.

For a simple subscriber, you just have to add a {{[Subscriber](Subscriber)}} attribute to your class:

{{
[Subscriber](Subscriber)
public class MySubscriber
{
    ...
}
}}

To actually receive data from a publisher, you have to indicate your interest in a certain type.
Let's assume that our subscriber is interested in receiving status messages with a certain custom type

{{
[Serializable](Serializable)
public class Status
{
    public string StatusMessage{get; set;}

    public int NodeID{get; set;}

    public DateTime Timestamp {get; set;}
}
}}
_Note that every data object which should be transferred by Ella must be serializable!! this is not necessary if you operate locally only._
Then the subscriber will look sth. like this

{{
[Subscriber](Subscriber)
public class StatusReceiver
{
    public void Init()
    {
        Subscribe.To<Status>(this, OnNewStatus);
    }

    public void OnNewStatus(Status status)
    {
        //Process the status message
        ...
    }
}
}}

After the Call to {{Subscribe.To}} Ella will immediately check for suitable publishers. This is done locally and on each known Ella node in the network. As soon as any suitable publisher publishes a new Status, it will be delivered to our {{StatusReceiver}}.

**That's it**, you've successfully subscribed, all the rest (networking, discovery, serialization and transport, ....) is done by Ella.

## Optional parameters for subscribing
You can add further parameters to the call to {{Subscribe.To}}.

**evaluateTemplateObject**: here you pass a delegate to your code which evaluated Tepmplate objects. I.e. Ella will ask every suitable publisher to provide a template object, and only if your function returns {{true}} for one template, your subscriber will be subscribed to it.

**DataModifyPolicy**: Here you can indicate, that you will modify the data which you receive from a publisher. In local operation, you might destroy data which another publisher or subscriber still needs (imagine passing images around and you change pixel values). If necessary, Ella will duplicate each published object before passing it to subscribers in order to prevent accidental data loss. This necessity is determined by comparing the DataModifyPolicy of each subscriber and the DataCopyPolicy of the publisher.

**forbidRemote**: if {{true}} Ella will not search for suitable publishers on remote nodes.

**subscriptionCallback**: You can pass a delegate to a method which must be called each time your subscriber is subscribed to a publisher. This will give you a SubscriptionHandle object which you can use to distinguish between events coming from differen publishers. So you could distinguish between a StatusObject of a publisher on Node 1 and Node 2.

