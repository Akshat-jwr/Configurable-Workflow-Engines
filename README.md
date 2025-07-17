# Infonetica-Task: Configurable Workflow Engine

A minimal backend service for managing configurable state-machine workflows built with .NET 8.

## Quick Start

### Prerequisites
- .NET 8 SDK installed
- Any code editor (VS Code recommended)

### Run the Application
```bash
git clone https://github.com/Akshat-jwr/Configurable-Workflow-Engines.git
cd Infonetica-Task
dotnet run
```


The API will be available at: `http://localhost:5000`

## API Endpoints

### Workflow Definitions

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/workflows` | Create a new workflow definition |
| `GET` | `/api/workflows/{id}` | Get workflow definition by ID |
| `GET` | `/api/workflows` | List all workflow definitions |
| `PUT` | `/api/workflows/{id}` | Update workflow definition |

### Workflow Instances

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/workflows/{workflowId}/instances` | Start new workflow instance |
| `GET` | `/api/instances/{id}` | Get instance details with history |
| `GET` | `/api/instances` | List all workflow instances |
| `GET` | `/api/workflows/{workflowId}/instances` | Get instances by workflow |
| `POST` | `/api/instances/{instanceId}/transitions` | Execute transition on instance |

## Testing Guide
You can test these on postman or bash

### Step 1: Create a Workflow Definition

**POST** `http://localhost:5000/api/workflows`
```json
{
"id": "order-workflow",
"name": "Order Processing",
"states": [
{
"id": "pending",
"name": "Order Pending",
"isInitial": true,
"isFinal": false,
"enabled": true
},
{
"id": "processing",
"name": "Processing Order",
"isInitial": false,
"isFinal": false,
"enabled": true
},
{
"id": "completed",
"name": "Order Completed",
"isInitial": false,
"isFinal": true,
"enabled": true
}
],
"transitions": [
{
"id": "start-processing",
"name": "Start Processing",
"enabled": true,
"fromStates": ["pending"],
"toState": "processing"
},
{
"id": "complete-order",
"name": "Complete Order",
"enabled": true,
"fromStates": ["processing"],
"toState": "completed"
}
]
}

```

### Step 2: Start a Workflow Instance

**POST** `http://localhost:5000/api/workflows/order-workflow/instances`
```json
{
"instanceId": "order-12345"
}
```

### Step 3: Execute Transitions

**POST** `http://localhost:5000/api/instances/order-12345/transitions`
```json
{
"transitionId": "start-processing"
}
```

Then execute the next transition:
```json
{
"transitionId": "complete-order"
}
```

### Step 4: Check Instance Status

**GET** `http://localhost:5000/api/instances/order-12345`

You'll see the current state and complete history of transitions.

## Core Concepts (Models I used)

### State
- **id**: Unique identifier
- **name**: Display name
- **isInitial**: True for starting state (exactly one per workflow)
- **isFinal**: True for ending states
- **enabled**: Whether the state is active

### Transition
- **id**: Unique identifier
- **name**: Display name
- **enabled**: Whether transition can be executed
- **fromStates**: Array of valid source states
- **toState**: Target state (single state only)

### Workflow Definition
- Collection of states and transitions
- Must have exactly one initial state
- All transitions must reference valid states

### Workflow Instance
- Running copy of a workflow definition
- Tracks current state and execution history
- Starts at the initial state

## Validation Rules (To ensure perfectly error-less transitions and updates)

### Workflow Definition
- Must have exactly one initial state
- State IDs must be unique
- Transition IDs must be unique
- All transition references must be valid states

### Transition Execution
- Transition must exist in workflow
- Transition must be enabled
- Current state must be in transition's fromStates
- Cannot execute transitions on final states

## Error Responses

All errors return appropriate HTTP status codes with JSON responses:
```json
{
"error": "Validation Error",
"message": "Detailed error description"
}
```

## Testing with Postman/curl

### Example curl Commands

**Create workflow:**
```bash
curl -X POST http://localhost:5000/api/workflows
-H "Content-Type: application/json"
-d @workflow.json
```

**Start instance:**
```bash
curl -X POST http://localhost:5000/api/workflows/order-workflow/instances
-H "Content-Type: application/json"
-d '{"instanceId": "order-123"}'
```

**Execute transition:**
```bash
curl -X POST http://localhost:5000/api/instances/order-123/transitions
-H "Content-Type: application/json"
-d '{"transitionId": "start-processing"}'
```

