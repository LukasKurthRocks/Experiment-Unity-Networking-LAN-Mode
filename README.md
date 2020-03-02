# LAN Multiplayer
Gefühlt mein zehntes Netzwerk Projekt. Müsste mal meinen Unity Ordner ausmisten ...

## Aktuelles
Dieses Projekt sollte als Test für mich dienen, unter anderem um die grundlegenden Funktionen des
Networking in C# zu erlernen. Somit dient dieses Projekt mehr als Beispiel wie manches funktionieren könnte.
Das meiste davon sollte lieber pro Projekt neu aufgebaut werden (Die Spezifikationen für das Netzwerk hängen
sowieso vom Spiel ab...).

Du kannst Dich dennoch frei fühöen und das Projekt clonen um es nach Deinen Vorstellungen zu misbrauchen.

Kleine Anhaltspunkte:
* #8 von Toms Serie ist nicht implementiert
* Beim stoppen des Hosts wird der Spieler nicht gekillt (habe ich noch keine Lust zu zu suchen).

### Zu Erledigen
* [Netzwerk Sachen rauslesen und vereinen (Merge)](https://github.com/LukasKurthRocks/Unity-Network-Client-V1)?

## Info
Ziel ist es einen LAN Modus zu haben, damit man für kleine Spiele mit Freunden nicht auf
einen Server warten muss. Eventuell geht der Server irgendwann auch nicht mehr und dann
möchte man vielleicht trotzdem weiter spielen.

## Kleine Hürden

### "Traue niemandem!"
"Eigentlich" (tolles Wort) wollte ich die Client/Server-Projekte von Tom Weiland kombinieren
und somit alles durch ein Projekt erstellen lassen. Hatte ich an sich auch getan, funktionierte auch.
Dann gab mir jemand namens "KAS" auf dem Discord von Tom allerdings einen Schubs in die richtige
Richtung:

> The client can't be trusted, the server can't be either

Könnte man jedem Menschen auf diesem Planeten trauen, könnte man auch alles in einem Projekt
laufen lassen. Da dem aber leider nicht so ist, sollte man den "Autoritativen Server" von der
Client Seite/dem Client Projekt trennen. Muss daher ein kleines LAN Projekt erstellen, damit
ich einen LAN Modus einbauen kann.

Dazu muss ich das ganze LAN Konzept allerdings erst einmal abstrahieren. Ich meine in der Theorie
das ganze Netzwerk Gedöns verstanden zu haben, aber es ist noch ein weiter Weg um ein
Pro in Sachen Netzwerk und C# zu sein (nicht, dass das wirklich mein Ziel wäre).

### Was wird aus DOTS?
Unity baut gerade DOTS weiter aus. Bis auf das [DOTS Sample](https://github.com/Unity-Technologies/DOTSSample)
und ein paar Erklärungen, wie das neue System alles revolutionieren und besser machen soll - und möglicherweise auch wird -
gibt es bis jetzt nur leider wenig Informationen und Dokumentationen zu diesem Thema. Es gibt viele Leute die das
Ganze interessieren wird, aber bisher keinen der bei Problemen - die beim Ableiten aus dem Sample und dem Experimentieren
auftreten - helfen kann.

Ich bin gespannt was darauf wird, vielleicht kann man dann auch einen LAN Modus mit zwei Klick erstellen, was das kleine
Projekt hier überflüssig machen wird...

___
## Quellen
### Links
* https://stackoverflow.com/questions/37951902/how-to-get-ip-addresses-of-all-devices-in-local-network-with-unity-unet-in-c
* https://forum.unity.com/threads/c-detecting-connected-devices-through-lan.297115/
* https://github.com/Unity-Technologies/DOTSSample
* https://github.com/Unity-Technologies/EntityComponentSystemSamples
* https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
* https://www.codeproject.com/Articles/16935/A-Chat-Application-Using-Asynchronous-UDP-sockets
* https://forum.unity.com/threads/c-detecting-connected-devices-through-lan.297115/
