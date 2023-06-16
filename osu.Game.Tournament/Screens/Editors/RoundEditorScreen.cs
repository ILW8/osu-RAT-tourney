// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class RoundEditorScreen : TournamentEditorScreen<RoundEditorScreen.RoundRow, TournamentRound>
    {
        protected override BindableList<TournamentRound> Storage => LadderInfo.Rounds;

        public partial class RoundRow : CompositeDrawable, IModelBacked<TournamentRound>
        {
            public TournamentRound Model { get; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; }

            public RoundRow(TournamentRound round)
            {
                Model = round;

                Masking = true;
                CornerRadius = 10;

                RoundBeatmapEditor beatmapEditor = new RoundBeatmapEditor(round)
                {
                    Width = 0.95f
                };

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Padding = new MarginPadding { Right = 160 },
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox
                            {
                                LabelText = "Name",
                                Width = 0.33f,
                                Current = Model.Name
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Description",
                                Width = 0.33f,
                                Current = Model.Description
                            },
                            new DateTextBox
                            {
                                LabelText = "Start Time",
                                Width = 0.33f,
                                Current = Model.StartDate
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Best of",
                                Width = 0.33f,
                                Current = Model.BestOf
                            },
                            new SettingsButton
                            {
                                Width = 0.2f,
                                Margin = new MarginPadding(10),
                                Text = "Add beatmap (developer 1)",
                                Action = () => beatmapEditor.CreateNew()
                            },
                            new SettingsButton
                            {
                                Width = 0.2f,
                                Margin = new MarginPadding(10),
                                Text = "Add beatmap (developer 2)",
                                Action = () => beatmapEditor.CreateNew(1)
                            },
                            beatmapEditor
                        }
                    },
                    new DangerousSettingsButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.None,
                        Width = 150,
                        Text = "Delete Round",
                        Action = () =>
                        {
                            Expire();
                            ladderInfo.Rounds.Remove(Model);
                        },
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public partial class RoundBeatmapEditor : CompositeDrawable
            {
                private readonly TournamentRound round;
                private readonly FillFlowContainer flow;
                private readonly FillFlowContainer flow2;

                public RoundBeatmapEditor(TournamentRound round)
                {
                    this.round = round;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Margin = new MarginPadding(8),
                        Children = new Drawable[]
                        {
                            new TournamentSpriteText
                            {
                                Text = "Developer 1 pool",
                                Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                                Padding = new MarginPadding { Left = 8 }
                            },
                            flow = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                ChildrenEnumerable = round.Beatmaps.Select(p => new RoundBeatmapRow(round, p))
                            },
                            new TournamentSpriteText
                            {
                                Text = "Developer 2 pool",
                                Font = OsuFont.Torus.With(weight: FontWeight.Bold, size: 24),
                                Padding = new MarginPadding { Left = 8 }
                            },
                            flow2 = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                ChildrenEnumerable = round.Beatmaps2.Select(p => new RoundBeatmapRow(round, p))
                            },
                        }
                    };
                }

                public void CreateNew(int listIndex = 0)
                {
                    var roundBeatmap = new RoundBeatmap();

                    switch (listIndex)
                    {
                        case 1:
                            flow2.Add(new RoundBeatmapRow(round, roundBeatmap));
                            round.Beatmaps2.Add(roundBeatmap);
                            break;

                        default:
                            flow.Add(new RoundBeatmapRow(round, roundBeatmap));
                            round.Beatmaps.Add(roundBeatmap);
                            break;
                    }
                }

                public partial class RoundBeatmapRow : CompositeDrawable
                {
                    public RoundBeatmap Model { get; }

                    [Resolved]
                    protected IAPIProvider API { get; private set; }

                    private readonly Bindable<int?> beatmapId = new Bindable<int?>();

                    private readonly Bindable<string> mods = new Bindable<string>(string.Empty);

                    private readonly Container drawableContainer;

                    public RoundBeatmapRow(TournamentRound team, RoundBeatmap beatmap)
                    {
                        Model = beatmap;

                        Margin = new MarginPadding(10);

                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        Masking = true;
                        CornerRadius = 5;

                        InternalChildren = new Drawable[]
                        {
                            new Box
                            {
                                Colour = OsuColour.Gray(0.2f),
                                RelativeSizeAxes = Axes.Both,
                            },
                            new FillFlowContainer
                            {
                                Margin = new MarginPadding(5),
                                Padding = new MarginPadding { Right = 160 },
                                Spacing = new Vector2(5),
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = "Beatmap ID",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = beatmapId,
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = "Mods",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = mods,
                                    },
                                    drawableContainer = new Container
                                    {
                                        Size = new Vector2(100, 70),
                                    },
                                }
                            },
                            new DangerousSettingsButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.None,
                                Width = 150,
                                Text = "Delete Beatmap",
                                Action = () =>
                                {
                                    Expire();
                                    team.Beatmaps.Remove(beatmap);
                                },
                            }
                        };
                    }

                    [BackgroundDependencyLoader]
                    private void load()
                    {
                        beatmapId.Value = Model.ID;
                        beatmapId.BindValueChanged(id =>
                        {
                            Model.ID = id.NewValue ?? 0;

                            if (id.NewValue != id.OldValue)
                                Model.Beatmap = null;

                            if (Model.Beatmap != null)
                            {
                                updatePanel();
                                return;
                            }

                            var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = Model.ID });

                            req.Success += res =>
                            {
                                Model.Beatmap = new TournamentBeatmap(res);
                                updatePanel();
                            };

                            req.Failure += _ =>
                            {
                                Model.Beatmap = null;
                                updatePanel();
                            };

                            API.Queue(req);
                        }, true);

                        mods.Value = Model.Mods;
                        mods.BindValueChanged(modString => Model.Mods = modString.NewValue);
                    }

                    private void updatePanel() => Schedule(() =>
                    {
                        drawableContainer.Clear();

                        if (Model.Beatmap != null)
                        {
                            drawableContainer.Child = new TournamentBeatmapPanel(Model.Beatmap, Model.Mods)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Width = 300
                            };
                        }
                    });
                }
            }
        }

        protected override RoundRow CreateDrawable(TournamentRound model) => new RoundRow(model);
    }
}
