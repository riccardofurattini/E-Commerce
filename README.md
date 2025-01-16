# E-Commerce

ho implementato 2 microservizi

il primo è Magazzino che si occupa della gestione degli item presenti, e ho esposto le api per creare un item, modificarlo ed eliminarlo.
inoltre è presente un api per visulizzare tutti gli items o un singolo item cercandolo con l'id

il secondo microservizio sviluppato è Store, che si occupa della comunicazione tra il cliente e il magazzino, infatti ho sviluppato le api per visualizzare il catalogo di articoli presenti in magazzino e le api per aggiungere/rimuovere articoli dal carrello.
tra store e magazzino è presente una comunicazione asincrona tramite kafka che tiene aggiornato in tempo reale il catalogo di store.

