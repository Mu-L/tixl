using T3.Editor.Gui.UiHelpers;

namespace T3.Editor.SkillQuest;

internal static class SkillQuestStates
{
    internal static State<SkillQuestContext> InActive
        = new(
              Enter: context => { },
              Update: context => { },
              Exit:
              _ => { }
             );
    
    internal static State<SkillQuestContext> Playing
        = new(
              Enter: _ => { },
              Update: context =>
                      {

                      },
              Exit: _ => { }
             );
    
    internal static State<SkillQuestContext> Completed
        = new(
              Enter: _ => { },
              Update: context => { },
              Exit: _ => { }
             );
}