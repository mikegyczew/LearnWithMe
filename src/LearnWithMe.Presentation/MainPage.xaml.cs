using LearnWithMe.Application.Features.Alphabet;
using LearnWithMe.Application.Features.Math;
using LearnWithMe.Domain.Entities;
using LearnWithMe.Presentation.ViewModels;
using LearnWithMe.Presentation.Drawing;
using Microsoft.Maui.Graphics;

namespace LearnWithMe.Presentation;

public partial class MainPage : ContentPage
{
	private readonly HomeViewModel _viewModel;
	private readonly LetterTracingDrawable _tracingDrawable = new();
	private readonly List<PointF> _tracePoints = [];

	public MainPage(HomeViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
		UpdateLearningZoneVisibility();
		viewModel.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName is nameof(HomeViewModel.IsLearningVisible) or nameof(HomeViewModel.IsMathVisible))
			{
				UpdateLearningZoneVisibility();
			}
		};
		TracingCanvas.Drawable = _tracingDrawable;
		NumberTracingCanvas.Drawable = _tracingDrawable;
		InitializeTracingCanvas(TracingCanvas);
		InitializeTracingCanvas(NumberTracingCanvas);
		viewModel.Learning.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(AlphabetLearningViewModel.SelectedLetter))
			{
				_tracingDrawable.Letter = viewModel.Learning.SelectedLetter?.Uppercase ?? "?";
				_tracePoints.Clear();
				_tracingDrawable.Points = _tracePoints;
				TracingCanvas.Invalidate();
			}
		};
		viewModel.Math.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == nameof(MathLearningViewModel.Question))
			{
				_tracingDrawable.Letter = viewModel.Math.Question?.CorrectAnswer.ToString() ?? "?";
				_tracePoints.Clear();
				_tracingDrawable.Points = _tracePoints;
				NumberTracingCanvas.Invalidate();
			}
		};
		_tracingDrawable.Letter = viewModel.Math.Question?.CorrectAnswer.ToString() ?? "?";
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.LoadAsync();
	}

	private async void OnProfileSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		await _viewModel.ActivateSelectedProfileAsync();
	}

	private async void OnDeleteProfileClicked(object? sender, EventArgs e)
	{
		if (sender is not Button { BindingContext: ChildProfile profile })
		{
			return;
		}

		var confirmed = await DisplayAlert(
			"Usunąć profil?",
			$"Profil {profile.Name}, jego postępy i historia ćwiczeń zostaną trwale usunięte.",
			"Usuń",
			"Anuluj");
		if (!confirmed)
		{
			return;
		}

		await _viewModel.Profiles.DeleteProfileAsync(profile);
		await _viewModel.ActivateSelectedProfileAsync();
	}

	private void OnAlphabetLetterClicked(object? sender, EventArgs e)
	{
		if (sender is Button { BindingContext: AlphabetLetter letter })
		{
			_viewModel.Learning.SelectLetterCommand.Execute(letter);
		}
	}

	private void OnExerciseAnswerClicked(object? sender, EventArgs e)
	{
		if (sender is Button { BindingContext: string answer })
		{
			_viewModel.Learning.AnswerCommand.Execute(answer);
		}
	}

	private void OnMathAnswerClicked(object? sender, EventArgs e)
	{
		if (sender is Button { BindingContext: int answer })
		{
			_viewModel.Math.AnswerCommand.Execute(answer);
		}
	}

	private void OnNumberExampleSpeakClicked(object? sender, EventArgs e)
	{
		if (sender is Button { BindingContext: NumberReadingExample example })
		{
			_viewModel.Math.SpeakTextCommand.Execute(example.Phrase);
		}
	}

	private void UpdateLearningZoneVisibility()
	{
		LearningContainer.IsVisible = _viewModel.IsLearningVisible && !_viewModel.IsMathVisible;
		MathContainer.IsVisible = _viewModel.IsLearningVisible && _viewModel.IsMathVisible;
	}

	private void InitializeTracingCanvas(GraphicsView canvas)
	{
		canvas.StartInteraction += OnTraceStarted;
		canvas.DragInteraction += OnTraceDragged;
		canvas.EndInteraction += OnTraceEnded;
	}

	private void OnTraceStarted(object? sender, TouchEventArgs e)
	{
		if (sender is not GraphicsView canvas)
		{
			return;
		}

		if (_tracePoints.Count > 0)
		{
			_tracePoints.Add(new PointF(float.NaN, float.NaN));
		}

		AddTracePoint(e, canvas);
	}

	private void OnTraceDragged(object? sender, TouchEventArgs e)
	{
		if (sender is GraphicsView canvas)
		{
			AddTracePoint(e, canvas);
		}
	}

	private async void OnTraceEnded(object? sender, TouchEventArgs e)
	{
		if (sender is not GraphicsView canvas)
		{
			return;
		}

		AddTracePoint(e, canvas);
		canvas.Invalidate();
		if (IsTraceComplete(canvas))
		{
			if (canvas == NumberTracingCanvas)
			{
				await _viewModel.Math.RecordNumberTraceAsync(wasComplete: true);
			}
			else
			{
				await _viewModel.Learning.RecordTraceAsync(wasComplete: true);
			}
		}
		else if (_tracePoints.Count > 1)
		{
			if (canvas == NumberTracingCanvas)
			{
				await _viewModel.Math.RecordNumberTraceAsync(wasComplete: false);
			}
			else
			{
				await _viewModel.Learning.RecordTraceAsync(wasComplete: false);
			}
		}
	}

	private void OnClearTraceClicked(object? sender, EventArgs e)
	{
		ClearTrace(TracingCanvas);
	}

	private void OnClearNumberTraceClicked(object? sender, EventArgs e)
	{
		ClearTrace(NumberTracingCanvas);
	}

	private void ClearTrace(GraphicsView canvas)
	{
		_tracePoints.Clear();
		canvas.Invalidate();
	}

	private void AddTracePoint(TouchEventArgs e, GraphicsView canvas)
	{
		if (e.Touches.Any())
		{
			_tracePoints.Add(e.Touches.First());
			canvas.Invalidate();
		}
	}

	private bool IsTraceComplete(GraphicsView canvas)
	{
		var canvasWidth = (float)canvas.Width;
		var canvasHeight = (float)canvas.Height;
		if (canvasWidth <= 0 || canvasHeight <= 0)
		{
			return false;
		}

		var points = _tracePoints
			.Where(point => float.IsFinite(point.X) && float.IsFinite(point.Y))
			.ToArray();
		if (points.Length < 2)
		{
			return false;
		}

		var minX = points.Min(point => point.X);
		var maxX = points.Max(point => point.X);
		var minY = points.Min(point => point.Y);
		var maxY = points.Max(point => point.Y);
		var pathLength = 0f;
		var occupiedCells = new HashSet<(int X, int Y)>();
		PointF? previousPoint = null;

		foreach (var point in _tracePoints)
		{
			if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
			{
				previousPoint = null;
				continue;
			}

			if (previousPoint is { } previous)
			{
				pathLength += MathF.Sqrt(
					MathF.Pow(point.X - previous.X, 2) +
					MathF.Pow(point.Y - previous.Y, 2));
			}

			var cellX = Math.Clamp((int)(point.X / canvasWidth * 4), 0, 3);
			var cellY = Math.Clamp((int)(point.Y / canvasHeight * 4), 0, 3);
			occupiedCells.Add((cellX, cellY));
			previousPoint = point;
		}

		return maxX - minX >= canvasWidth * 0.15f &&
			maxY - minY >= canvasHeight * 0.35f &&
			pathLength >= canvasHeight * 1.05f &&
			occupiedCells.Count >= 4;
	}
}
