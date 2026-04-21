# Random Video Game Generator

Scopri nuovi giochi in modo casuale! Un'applicazione moderna e intuitiva che ti consente di generare suggerimenti di videogiochi casuali, salvarli nella tua collezione e tradurre le descrizioni in italiano.

## 🎮 Descrizione del Progetto

**Random Video Game Generator** è un'applicazione web costruita con Vue 3 e Vite che integra l'API di [RAWG](https://rawg.io/) per fornire informazioni su migliaia di videogiochi. 

### Funzionalità Principali

- 🎲 **Generatore Casuale**: Genera suggerimenti di giochi casuali con un solo clic
- 💾 **Salva Giochi**: Salva i tuoi giochi preferiti in una collezione personale persistente
- 🔍 **Filtri Avanzati**: Filtra i giochi per genere, piattaforma e altre caratteristiche
- 🌍 **Traduzione Automatica**: Le descrizioni dei giochi vengono tradotte automaticamente in italiano
- 📊 **Informazioni Dettagliate**: Visualizza rating, data di rilascio, generi, piattaforme e screenshot
- 🎨 **Design Moderno**: Interfaccia con glassmorphism e animazioni fluide
- 📱 **Responsive**: Perfettamente ottimizzato per desktop, tablet e dispositivi mobili
- 💾 **Persistenza Locale**: I giochi salvati vengono memorizzati nel browser e rimangono disponibili

## 🛠️ Stack Tecnologico

- **Framework**: Vue 3 (Composition API)
- **Build Tool**: Vite
- **Styling**: Tailwind CSS
- **HTTP Client**: Axios
- **Linting**: ESLint
- **Formatter**: Prettier
- **Database Locale**: IndexedDB
- **API Esterna**: RAWG API

## 📋 Prerequisiti

- Node.js (versione 16 o superiore)
- npm o yarn
- Una chiave API RAWG (gratuita) da [rawg.io/api](https://rawg.io/api)

## 🚀 Installazione e Setup

### 1. Clonare il Repository

```bash
git clone <repository-url>
cd videogame-randomized
```

### 2. Installare le Dipendenze

```bash
npm install
```

### 3. Configurare le Variabili di Ambiente

Usa il file `.env` nella root del repository (template: `.env.example` nella root).

Esempio minimo:

```env
VITE_RAWG_API_KEY=YOUR_RAWG_API_KEY_HERE
```

### 4. Avviare il Server di Sviluppo

```bash
npm run dev
```

L'applicazione sarà disponibile su `http://localhost:5173`

## 📦 Script Disponibili

### Sviluppo

```bash
npm run dev
```

Avvia il server di sviluppo con hot-reload. Perfetto per lo sviluppo locale.

### Build per Produzione

```bash
npm run build
```

Compila l'applicazione per la produzione. I file ottimizzati verranno generati nella cartella `dist/`.

### Anteprima della Build

```bash
npm run preview
```

Avvia un server locale per visualizzare la build di produzione prima del deployment.

### Linting

```bash
npm run lint
```

Controlla e corregge automaticamente gli errori di stile del codice con ESLint.

### Formattazione

```bash
npm run format
```

Formatta il codice utilizzando Prettier per garantire uno stile coerente.

## 🏗️ Struttura del Progetto

```
src/
├── components/
│   ├── FilterSection.vue      # Sezione filtri (genere, piattaforma, ecc.)
│   ├── GameCard.vue           # Card del gioco con tutte le informazioni
│   ├── GameHistory.vue        # Cronologia dei giochi generati
│   └── SaveGamesModal.vue     # Modal per visualizzare i giochi salvati
├── services/
│   ├── api.js                 # Servizio per le chiamate API RAWG
│   ├── database.js            # Servizio per la gestione del database locale (IndexedDB)
│   └── translation.js         # Servizio per la traduzione delle descrizioni
├── assets/
│   └── main.css               # Stili globali
├── App.vue                    # Componente principale dell'applicazione
└── main.js                    # Punto di ingresso dell'applicazione
```

## 📝 Variabili di Ambiente

Le seguenti variabili di ambiente possono essere configurate:

| Variabile | Descrizione | Obbligatoria |
|-----------|-------------|--------------|
| `VITE_RAWG_API_KEY` | Chiave API per accedere a RAWG | Sì |

## 🔧 Configurazione IDE Consigliata

### VSCode

Si consiglia di usare [VSCode](https://code.visualstudio.com/) con le seguenti estensioni:

- [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) - Support per Vue 3 (disabilita Vetur se installato)
- [Tailwind CSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss)
- [ESLint](https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint)
- [Prettier](https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode)

## 🎯 Come Usare l'Applicazione

1. **Genera un Gioco Casuale**: Clicca il pulsante "Genera Gioco" per ottenere un suggerimento casuale
2. **Salva il Gioco**: Clicca il pulsante "Salva" sulla card del gioco per aggiungerlo alla tua collezione
3. **Visualizza Giochi Salvati**: Clicca il pulsante "Saved Games" in alto a destra per vedere la tua collezione
4. **Filtra i Giochi**: Usa i filtri nella sezione superiore per cercare giochi specifici per genere, piattaforma, ecc.
5. **Leggi le Descrizioni**: Le descrizioni dei giochi vengono automaticamente tradotte in italiano

## 🌐 Riferimenti Utili

- [Documentazione Vue 3](https://v3.vuejs.org/)
- [Documentazione Vite](https://vitejs.dev/)
- [Tailwind CSS](https://tailwindcss.com/)
- [RAWG API Documentation](https://rawg.io/api)

## 📄 Licenza

Questo progetto è open source e disponibile sotto licenza MIT.

## 👤 Autore

Creato da Ruben Pari

## 🤝 Contributi

I contributi sono benvenuti! Sentiti libero di aprire issue o pull request per migliorare l'applicazione.
