# Tasks Spec

## Project

- Name: Vibe Tasks
- Description: Desktop task management app with notes, habits, mind maps, journals, flashcards, spreadsheets, and draw.io diagrams
- Stack: Electron, Vite, React, TypeScript, SQLite

## Tasks

- Create, read, update, delete tasks
- Organize by status and priority
- Assign to users
- Recurring tasks with configurable intervals
- Archiving
- CSV/JSON import/export

## Resources

- `electron/database/` — SQLite DB layer
- `src/pages/` — React page components
- `electron/main.ts` — Main process + IPC handlers
- `electron/preload.cjs` — Context bridge
- `public/drawio/` — Self-hosted draw.io webapp
- `draw-io-todos.md` — Draw feature bug tracker
