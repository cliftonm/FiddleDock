# FiddleDock
An example of creating a Fiddle website using Docker containers to isolate code execution.

## The Beginnings
Using a combination of C# and Python, we explore a very simple solution to the problem of executing
unknown and potentially unsafe Python code by executing the Python code in a Docker container.

This code demonstrates one possible solution, which involves:

1. Starting each container from a baseline image.
2. Uploading the user's Python code to the container via a simple HTTP server using Flask.
3. Executing the code in the container.
4. Returning the results to the user.

* Containers are instantiated from the baseline image and mapped to a unique port for the user's session.  This is one 
such possible implementation to avoid port conflicts, another is to use different local IP addresses.  Other ways of
communicating to an executing container exist as well, but I haven't explored those.  Yet another approach might be to
utilize something like Nginx for routing to different docker containers, but again, this is beyond what I'm demonstrating here.

* Because this is a demonstration web-app, certain things are exposed on the front-end that you wouldn't normally expose, such
as Docker image ID's, the ability to create new containers within a single session, and the ability to stop/kill a container.

* The user-facing UI, implemented as a web page, is served by a C# back-end (because that's my language of choice) and the
Docker container HTTP server is written in Python (because that's my second language of choice.)

