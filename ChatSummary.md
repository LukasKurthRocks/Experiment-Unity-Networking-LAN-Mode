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
* Public Notes
  * http://minhhh.github.io/posts/unity-local-network-multiplayer (Possible Options)
  * https://forum.unity.com/threads/what-are-the-pros-and-cons-of-available-network-solutions-assets.609088/ => BIG COMPARISON
* P2P ("TL;DR: peer to peer is scary and building your own backend solution is difficult.")
  * DOTS ?
* ~~Bluetooth (suboptimal, insecure; most plugins expensive, outdated, not mantained; basically no tutorial for this)~~
  * https://assetstore.unity.com/packages/tools/network/bluetooth-le-for-ios-tvos-and-android-26661
  * https://assetstore.unity.com/packages/tools/network/bluetooth-networking-for-ios-tvos-and-android-124274
  * https://devblog.blackberry.com/2014/02/diary-of-a-unity-3d-newbie-bluetooth-low-energy-plugins (2014)
  * https://assetstore.unity.com/packages/tools/integration/native-android-toolkit-mt-139365 (Android Tools, isBluetoothActive(), no bluetooth functionality)
  * https://stackoverflow.com/a/18984325 (Android CAN NOT connect to iOS with Bluetooth, Apple prevents this)
* Minimized LAN/Server/Host/MasterClient
* Future: DOTS?
  * "To ensure the smoothest transition to the future DOTS-Netcode, get started today using the Preview UTP with sample netcode from the FPS sample"
  * [Deep dive into networking for Unity's FPS Sample game - Unite LA](https://www.youtube.com/watch?v=k6JTaFE7SYI)
* Frameworks
  * ~~DarkRift2~~
    * [DarkRift2 - Unity Asset Store](https://assetstore.unity.com/packages/tools/network/darkrift-networking-2-95309)
    * [DarkRift2 - Documentation](https://darkriftnetworking.com/DarkRift2/Docs/2.5.0/index.html)
    * [DarkRift2 - Discord Channel](https://discordapp.com/invite/cz2FQ6k)
    * [DarkRift2 - HitHub Wiki](https://github.com/DarkRiftNetworking/DarkRift/wiki)
    * [DarkRift2 - Unity Forums](https://forum.unity.com/threads/darkrift-networking-2.516271/)
      * "DarkRift will work fine over LAN with a PC as the server but it sounds like you're intending to have the server on one of the mobile devices which isn't supported. If that is the case then you would probably want to host a relay server in the cloud for the two mobile devices to connect to and communicate via. "
  * ~~https://github.com/lidgren/lidgren-network-gen3~~
* Other
  * https://github.com/Unity-Technologies/EntityComponentSystemSamples
