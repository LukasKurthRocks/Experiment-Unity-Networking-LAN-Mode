## Notes from messages
* Client "world data" on the server
  * You could send all the shapes, but it wont be smart
  * Copy terrain over
* Seperate "important" server data from client
  * Steam development ID (maybe he meant the api key for purchasing system!?)
    * "post get request and ruin game page"
* Nowadays even "locale e-sport events" are via online servers
  * Might be a point, but i want LAN!
* Server side control panel when having game, locking framerate etc.
* Playfab insecure account system (click and see password?)
* BCrypt is "uncrackable" (his words)
  * "Only thing the client does, is encrypts the data he sends on the login packet, sends the iv and key with it 
  (This is massively insecure i know, still thinking on a solution) and server decrypts that"
  * "If you do clientside people can see the cost, modify the hash..."
  * "Just remember this sentence: "The client cant be trusted, the server cant be either"."
  * "Clients encrypt registration data -> sends it to server -> server decrypts registration data with the provided
  key and iv -> hashes password into database"
* "Yeah, people need to learn that [network security] yes, but i don't like to talk about my securing
systems publicly too much. But ill think about it."
* "HTTPS is secure, Rest API Callbacks are not" (OAuth2; not ideal for games?)
  * "Well callbacks could be located and triggered manually"
* Side-Mention (Login System)

### Options:
* P2P ("TL;DR: peer to peer is scary and building your own backend solution is difficult.")
* ~~Bluetooth (suboptimal, insecure)~~
* Minimized LAN/Server/Host/MasterClient
