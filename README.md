# Dev blog

## 2023-03-10

Tjena bloggen! Kul att vara här, riktigt taggad på att igång med dethär projektet (då jag skriver bloggen idag, den 10
mars 2023).
Mitt mål är att göra ett ascoolt nät som man kan kasta/släppa bollar i.

Idag har jag hoppat direkt in i repkoden och försökt klura ut vad som är vad, direkt känns det som att koden inte är så
skalbar och kanske kommer behöva lite kärlek för att sätta igång med.

## 2023-03-12

Jag har lyckats ankra repet i två separata punkter så att det hänger tvärs över som illustrerat, jag är mycket nöjd och
kommer klara detta ezpz.

Nästa mål är att länka flera rep tillsammans så att alla `RopePoint`s är sammankopplade både horisontellt och vertikalt.

## 2023-03-12 (igen)

Koden visar sig vara jobbigare att arbeta med än väntat, men jag har lyckats sammankoppla flera rep genom en extremt
hacky metod och genom att ge grannar till varje `RopePoint` som en lista.

## ~~2023-03-14~~ NY TITEL: KODEN ÄR SKIT

Koden är väldigt oskalbar (likt en omogen banan) och jag lyckas inte klura ut hur meshes fungerar överhuvudtaget. Likt
en medeltida by som stöter på en tidsresande från framtiden så kastade jag allt i sjön och låtsas som inget. ALla vet
att nät består av flytande punkter som är lite vagt nära varandra.

## Kollsioner

Jag har skrivit kod som hanterar kollisioner kring `RopePoint`-sen och låter dem kollidera med `Spheres`. Koden bygger
helt och hållet på fysikmotorn definierad i `Rope` och är <sub>~~skit~~</sub> fullt fungerande om än lite hackig. Målet är nu att få fysiken att fungera lite mer *smooth*<sup>TM</sup> och verklighetstroget.

## *smooth*<sup>TM</sup>-infusion
Jag skrev om allt till att använda Unitys fysikmotor. Koden är nu 100% min och jag tar all cred för den. Släng er i väggen modsim-assar (och tack för meshsen!)
