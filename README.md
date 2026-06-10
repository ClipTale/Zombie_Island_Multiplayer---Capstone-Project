# 2D Co-Op Networked Zombie Shooter

A networked multiplayer top-down survival game built in Unity, featuring real-time client-server synchronization, dedicated Node.js backend logic, and persistent SQL database state management.

## Architecture & Network Topology

The project utilizes a decoupled client-server architecture where the Unity client connects to a custom Node.js server instance handling remote connections, state routing, and transactional data persistence.

 **Hybrid Host/Client Topology:** * Dedicated peer-to-peer connection options allowing players to act as a Host or input a remote Host IP address to establish an online socket layer.
     Real-time network translation handling player movement tracking, dynamic zombie wave tracking, and shared combat state synchronization.
 **Node.js Game Server:**
     Orchestrates game session handshakes, dynamically routes network payloads between connected instances, and communicates directly with the database engine.
     Manages a dedicated web server instance rendering localized user data via structured HTTP endpoints (`localhost:6969`).
 **Relational Database Schema (MySQL):**
     Uses a `player_accounts` relational model tracking distinct entity metrics across sessions: `username`, `password`, `round`, `enemies_killed`, `deaths`, `times_played` and `highest_round`.

## Core Mechanics & Analytics Pipeline

 **State-Driven Survival Loop:** Top-down shooter gameplay where players coordinate to eliminate pathfinding zombie waves that scale dynamically per round.
 **Transactional State Persistence:**
     **Session Tracking:** Starting a game loop fetches the Host’s historical progression data ("Load Game") and automatically increments the lifetime global connection index (`times_played`) inside the database schema.
     **Post-Round Analytics Writing:** The host triggers a bulk transaction upon round completion to commit active metrics ("Save Game"). This updates current round parameters, tracks cumulative kill differentials, registers active deaths, and evaluates high-score boundaries (`highest_round`) using server-side logic.
 **Authentication Security Model:** Integrated login mechanics processing input fields directly against the secure database table to authorize profile retrieval and system state loading.
