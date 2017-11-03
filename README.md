# Ella
Welcome. Ella is a middleware project for the .NET ecosystem.
<div class="wikidoc">

<h1></h1>

<h1>Ella publish/subscribe middleware</h1>

<p>Ella is a fully distributed publish/subscribe (see <a href="/ella/wikipage?title=http%3a%2f%2fen.wikipedia.org%2fwiki%2fPublish-subscribe_pattern&referringTitle=Home">

http://en.wikipedia.org/wiki/Publish-subscribe_pattern</a> for the basic concept) middleware written in pure C# and compatible with Mono.<br>

<br>

It is also a very useful tool to build non-distributed applications. The Networking part can be completely disabled. Then Ella is used to enable composability, scalability, flexibility for your application.<br>

<br>

Ella handles all the communication needed to transfer data from a producer to a consumer with no need to care if both are on the same or on different nodes in your network. It discoveres other nodes which run Ella instances, so you do not have to care about

 how to scale your application.<br>

<br>

Ella is based on the type-based pub/sub flavour because it is most intuitive to handle and does not require topic conventions (in topic-based pub/sub) or a dedicated query mechanism for properties (in case of content-based pub/sub).</p>


<h2>Basic concepts</h2>

<p><em>An introductory article to Ella has been posted on <a href="http://www.codeproject.com/Articles/655774/Ella-publish-subscribe-middleware" target="_blank">

Codeproject</a>.</em></p>

<p>The goal in designing Ella was to make it most easy to use for developers. Thus, it does not require any inheriting from base classes or instance handling (you'll never see an Ella-object you have to use to call Ella-specific methods). Instead, we use Attributes

 to mark classes as publishers or subscribers (and for other stuff too) and provide a static facade to interact with the Middleware System. This facade is designed with intuitive and fluent use in mind.<br>

<br>

So you'll see a lot of code like<br>

<span class="codeInline">Subscribe.To&lt;String&gt;()</span><br>

or<br>

<span class="codeInline">Start.Publisher(p)</span>.</p>

<h2>Why &quot;Ella&quot;?</h2>

<p>You might ask why we call our middleware Ella......<br>

<strong>The short answer</strong>: Because we can.<br>

<strong>The Long answer</strong>: ever tried to find a meaningful name (or acronym) for a project? Then you know the problem. Instead of trying to fit the words &quot;publish, subscribe, middleware,....&quot; into an acronym we decided to go for a different approach.

 Being big fans of Jazz music, we decided to name the system after the greatest Jazz singer in history: Ella Fitzgerald. That's the story. It also makes it very easy to find release code names ;)</p>

</div><div class="ClearBoth"></div>

# Further documentataion
[To be found here](docs/Documentation.md)
