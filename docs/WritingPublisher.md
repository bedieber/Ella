# Implementing a publisher

Publishers are rather simple to implement. Every publisher has to define a {{[Factory](Factory)}}, a {{[Start](Start)}} and a {{[Stop](Stop)}} attribute. 

For defining a class as a publisher, you just have to add a {{[Publishes](Publishes)}} attribute to your class. This attribute contains additional information including the type that is published, a unique event ID and the copy policy, which indicates whether the published object has to be copied at modification or not. 

A simple example is shown below, where the class is defined as a publisher of type String with event ID 1 and no need for copying at modification.

{{
[Publishes(typeof(String), 1, CopyPolicy = DataCopyPolicy.None)](Publishes(typeof(String),-1,-CopyPolicy-=-DataCopyPolicy.None))
public class MyPublisher
{
    ...
}
}}

Now let's define the {{[Factory](Factory)}}, the {{[Start](Start)}} and the {{[Stop](Stop)}} attribute needed for every publisher. For this purpose we just have to implement three methods (one of each defining one of the attributes). You just have to add the attribute to the method in order to let the method define this specific attribute.

{{
[Factory](Factory)
public MyPublisher
{
    ...
}

[Start](Start)
public void Run()
{
    ...
}

[Stop](Stop)
public void Stop()
{
    ...
}
}}

Now we would like to publish something. Since we are publishers of type String we are now going to publish "Hello World". We can do this e.g. in the Start() method, but for now we implement a new method called PublishEvent(). 

{{
internal void PublishEvent()
{
        string publishEvent = "Hello World";

        Publish.Event(publishEvent, this, 1);
}
}}

As you can see from the example above, we can easily publish events by calling Publish.Event(event, publisher, eventID).

For creating a publisher, that's all you have to know. The networking, the discovery of subscribers and the delivery of events to appropriate subscribers is done by Ella. 



