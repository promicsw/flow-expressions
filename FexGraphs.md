# Flow Expression Graphs

### Class Diagram
```mermaid
classDiagram
  FexElement <|-- Seq
  FexElement <|-- Opt
  FexElement <|-- OneOf
  FexElement <|-- OptOneOf
  FexElement <|-- Rep
  FexElement <|-- RepOneOf
```

### Sequence:
```mermaid
graph LR
  subgraph "Seq (Sequence)" 
    direction LR
    A[FexElement 1] --> B[FexElement 2] -.-> C[FexElement n];
  end
```

### Optional:
```mermaid
graph LR
  subgraph "Opt (Optional)" 
    A[Seq]
  end
```

### OneOf:
```mermaid
graph LR
  subgraph OneOf
    direction LR
    A[Seq 1] -->|or| B[Seq 2] -.->|or| C[Seq 3];
  end
```

### OptOneOf:
```mermaid
graph LR
  subgraph OptOneOf
    direction LR
    A[Seq 1] -->|or| B[Seq 2] -.->|or| C[Seq 3];
  end
```

### Repeat:
```mermaid
graph LR
  subgraph "Rep (Repeat: min, max)"
    direction LR
    A[Seq];
  end
```

### RepOneOf:
```mermaid
graph LR
  subgraph "RepOneOf (min, max)"
    direction LR
    A[Seq 1] -->|or| B[Seq 2] -.->|or| C[Seq 3];
  end
```