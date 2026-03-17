class PyForgeError(Exception):
    """Base exception for PyForge."""


class AgentRegistrationError(PyForgeError):
    """Raised when agent registration is invalid."""


class AgentNotFoundError(PyForgeError):
    """Raised when the referenced agent does not exist."""


class TaskNotFoundError(PyForgeError):
    """Raised when the task id is unknown."""


class NoCompatibleAgentError(PyForgeError):
    """Raised when no agent can process task capability constraints."""


class InvalidTaskStateError(PyForgeError):
    """Raised on invalid task state transitions."""
