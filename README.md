# MovieMasterWPF
An application that reads movie files and obtains metadata about the movies from themobiedb.org.

I created this application to teach myself WPF.

Things that might be interesting about this project:

* uses datasets as local database
* implements user selectable themes
* making web service calls asyncronously
* updating the UI asynchronously

So how this works is you have a directory with a bunch of movies in it, and they might have weird names or proper names (such as MyFavoriteMovie(1985).avi) where myfavoritemovie is the name of a real movie like "Star Wars" or something of that nature. The program then mines out that information from the title and looks up metadata for the movie on moviedb.org (movie poster, synopsis etc) sort of like a very rudimentary XBMC.
