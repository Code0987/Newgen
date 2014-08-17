// Server.js for Newgen HtmlApp package.
// © 2014 NS, Newgen.
// DO NOT DELETE/MODIFY THIS SCRIPT.

module.exports = function (data, callback) {
    try { // <-- For preventing app from crashing !

        // Imports.
        var edge = require("./../Edge/edge.js"),
            http = require("http"),
            url = require("url"),
            path = require("path"),
            fs = require("fs");

        // Create http server.
        var server = http.createServer(function (request, response) {
            try {

                response.end("Hello from edge (node inside .Net CLR) ! It's " + new Date() + ".");

                // Current url and file.
                var currentUrl = url.parse(request.url)
                  , filename = path.join(data.location, currentUrl.pathname);

                // Handle requests.
                if (currentUrl.path.search(data.metaKey) > -1) {
                    // Handle meta requests.

                    // For /Test
                    if (currentUrl.path.search("/Test") > -1)
                        response.end("Hello from edge (node inside .Net CLR) ! It's " + new Date() + ".");

                    // For /CloseHub
                    if (currentUrl.path.search("/CloseHub") > -1)
                        data.appCloseHub(currentUrl.href, function (error, result) {
                        });

                } else
                    // Handle static file requests.
                    path.exists(filename, function (exists) {
                        if (!exists) {
                            response.writeHead(404, { "Content-Type": "text/plain" });
                            response.write("404 Not Found\n");
                            response.end();
                            return;
                        }

                        fs.readFile(filename, "binary", function (error, file) {
                            if (error) {
                                response.writeHead(500, { "Content-Type": "text/plain" });
                                response.write(error + "\n");
                                response.end();
                                return;
                            }

                            response.writeHead(200);
                            response.write(file, "binary");
                            response.end();
                        });
                    });

            } catch (e) {
                // Eat
            }
        }).listen(data.port, function () {
            try {

                // Stop callback.
                /* // Call below is crashing app !
                callback(null, function (data, callback) {
                    server.close();
                    callback();
                });
                */

            } catch (e) {
                // Eat
            }
        });

    } catch (e) {
        // Eat
    }

    return server;
};
