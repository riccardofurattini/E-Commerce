# E-Commerce

ho implementato 2 microservizi Store e Magazzino

Magazzino si occupa della gestione degli item presenti nel magazzino, ho esposto le api per creare un item, modificarlo ed eliminarlo.
inoltre è presente un api per visulizzare tutti gli items o un singolo item cercandolo con l'id.
è presente un api per la modifica della quantità di un item presente nel magazzino che servirà al servizio store per "prenotare" la quantità di merce che si mette nel carrello

Store, si occupa della comunicazione tra il client e il Magazzino, ho sviluppato le api per visualizzare il catalogo di articoli presenti in magazzino e le api per aggiungere/rimuovere articoli dal carrello e tramite una chiamata http modifica in tempo reale la quantità presente in magazzino.

tra store e magazzino è presente una comunicazione sincrona e asincrona:
Asincrona tramite kafka, Store rimane aggiornato sugli item presenti in magazzino, infatti ad ogni modifica nel Magazzino viene effettuato un invio di un messaggio kafka nell'apposito topic.
Sincrona tramite una chiamata http store può modificare ogni volta che viene aggiunto/modificato/eliminato un articolo dal carrello la quantità disponibile nel magazzino.
