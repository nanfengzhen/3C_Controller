public enum StateType
{
    Idle,
    Walk,
    Run,
    Attack,
    Jump,
    RunJump,
    WalkJump,
    Sprint,
    SprintJump,
    Dash,
    Landing,
}

public class PlayerStateType:Singleton<PlayerStateType>
{

    public StateType stateType;
}
