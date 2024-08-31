using System;
using Godot;
using Phoenyx;

public partial class Renderer : MultiMeshInstance3D
{
    public override void _Process(double delta)
    {
        if (!Runner.Playing)
        {
            return;
        }

        Multimesh.InstanceCount = Runner.ToProcess;

        Transform3D transform = new Transform3D(new Vector3((float)Settings.NoteSize / 2, 0, 0), new Vector3(0, (float)Settings.NoteSize / 2, 0), new Vector3(0, 0, (float)Settings.NoteSize / 2), Vector3.Zero);

        for (int i = 0; i < Runner.ToProcess; i++)
        {
            Note note = Runner.ProcessNotes[i];

            float depth = (note.Millisecond - (float)Runner.CurrentAttempt.Progress) / (1000 * (float)Settings.ApproachTime) * (float)Settings.ApproachDistance / (float)Runner.CurrentAttempt.Speed;
            float alpha = Math.Clamp((1 - (float)depth / (float)Settings.ApproachDistance) / ((float)Settings.FadeIn / 100), 0, 1);
            
            if (Settings.FadeOut)
            {
                alpha -= ((float)Settings.ApproachDistance - depth) / ((float)Settings.ApproachDistance + (float)Constants.HitWindow * (float)Settings.ApproachRate / 1000);
            }

            if (!Settings.Pushback && note.Millisecond - Runner.CurrentAttempt.Progress <= 0)
            {
                alpha = 0;
            }
            
            int j = Runner.ToProcess - i - 1;
            Color color = Phoenyx.Skin.Colors[note.Index % Phoenyx.Skin.Colors.Length];
            
            transform.Origin = new Vector3(note.X, note.Y, -depth);
            color.A = alpha;
            Multimesh.SetInstanceTransform(j, transform);
            Multimesh.SetInstanceColor(j, color);
        }
    }
}