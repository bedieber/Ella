# Attributes in Ella
We use Attributes whenever possible to indicate certain code areas as relevant to Ella.
As an example, you have to add a {{[Subscriber](Subscriber)}} attribute to a class in order to declare it a subscriber.

There are attributes for
* Declaring classes as publishers and subscribers
* Factory methods
* Start and stop methods
* Message reception methods
* Event association
* Template data generator methods

You can take a look at the {{Ella.Attributes}} namespace for details.

## Why attributes?
Of course, we could also use interfaces and abstract classes as base for publishers and subscribers. However, think what this means for a developer: If you want to adapt existing code to run on Ella, you just attribute your classes a bit and that's about it (of course, you still need to add methods to publish and subscribe). If you're forced into a inheritance hierarchy on the other side, this requires you to adapt all your code, implement interfaces and change your own hierarchy. In many cases this is not even easily done because it would require multiple inheritance.
That's the reason for us to go for attributes. It may require more reflection to be done by the Ella developers, but it takes a lot of work off the shoulders of module developers.