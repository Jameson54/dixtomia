using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using NCalc;

namespace dixotomia
{
  public partial class Form1 : Form
  {
    private const int graphSteps = 500;
    private const int signCheckSteps = 1000;
    private const double defaultAccuracy = 0.0001;
    private const int marginSize = 40;

    private TextBox leftBoundBox = null!;
    private TextBox rightBoundBox = null!;
    private TextBox accuracyBox = null!;
    private TextBox formulaBox = null!;
    private PictureBox graphBox = null!;
    private Label resultLabel = null!;

    public Form1()
    {
      BuildInterface();
    }

    private void BuildInterface()
    {
      Text = "Dichotomy method - root search";
      Size = new Size(900, 650);

      MenuStrip mainMenu = new MenuStrip();

      ToolStripMenuItem calculateItem = new ToolStripMenuItem("Calculate");
      calculateItem.Click += OnCalculateClick;

      ToolStripMenuItem plotItem = new ToolStripMenuItem("Plot graph");
      plotItem.Click += OnPlotClick;

      ToolStripMenuItem clearItem = new ToolStripMenuItem("Clear");
      clearItem.Click += OnClearClick;

      mainMenu.Items.Add(calculateItem);
      mainMenu.Items.Add(plotItem);
      mainMenu.Items.Add(clearItem);

      MainMenuStrip = mainMenu;
      Controls.Add(mainMenu);

      Panel inputPanel = new Panel();
      inputPanel.Top = 30;
      inputPanel.Left = 10;
      inputPanel.Width = 870;
      inputPanel.Height = 60;

      Label leftLabel = new Label();
      leftLabel.Text = "a =";
      leftLabel.Left = 0;
      leftLabel.Top = 10;
      leftLabel.Width = 20;

      leftBoundBox = new TextBox();
      leftBoundBox.Left = 25;
      leftBoundBox.Top = 7;
      leftBoundBox.Width = 80;
      leftBoundBox.Text = "0";

      Label rightLabel = new Label();
      rightLabel.Text = "b =";
      rightLabel.Left = 120;
      rightLabel.Top = 10;
      rightLabel.Width = 20;

      rightBoundBox = new TextBox();
      rightBoundBox.Left = 145;
      rightBoundBox.Top = 7;
      rightBoundBox.Width = 80;
      rightBoundBox.Text = "3";

      Label accuracyLabel = new Label();
      accuracyLabel.Text = "e =";
      accuracyLabel.Left = 240;
      accuracyLabel.Top = 10;
      accuracyLabel.Width = 20;

      accuracyBox = new TextBox();
      accuracyBox.Left = 265;
      accuracyBox.Top = 7;
      accuracyBox.Width = 80;
      accuracyBox.Text = "0.0001";

      Label formulaLabel = new Label();
      formulaLabel.Text = "f(x) =";
      formulaLabel.Left = 360;
      formulaLabel.Top = 10;
      formulaLabel.Width = 45;

      formulaBox = new TextBox();
      formulaBox.Left = 410;
      formulaBox.Top = 7;
      formulaBox.Width = 440;
      formulaBox.Text = "x^2 + 2*x - 6";

      inputPanel.Controls.Add(leftLabel);
      inputPanel.Controls.Add(leftBoundBox);
      inputPanel.Controls.Add(rightLabel);
      inputPanel.Controls.Add(rightBoundBox);
      inputPanel.Controls.Add(accuracyLabel);
      inputPanel.Controls.Add(accuracyBox);
      inputPanel.Controls.Add(formulaLabel);
      inputPanel.Controls.Add(formulaBox);

      Controls.Add(inputPanel);

      graphBox = new PictureBox();
      graphBox.Left = 10;
      graphBox.Top = 100;
      graphBox.Width = 870;
      graphBox.Height = 460;
      graphBox.BorderStyle = BorderStyle.FixedSingle;
      graphBox.BackColor = Color.White;

      Controls.Add(graphBox);

      resultLabel = new Label();
      resultLabel.Left = 10;
      resultLabel.Top = 570;
      resultLabel.Width = 870;
      resultLabel.Height = 30;
      resultLabel.Font = new Font("Consolas", 11.0f, FontStyle.Bold);
      resultLabel.Text = "Enter data and press Calculate";

      Controls.Add(resultLabel);
    }

    private double ParseDoubleValue(string textValue)
    {
      string preparedText = textValue.Replace(',', '.');
      double parsedValue = double.Parse(preparedText, CultureInfo.InvariantCulture);
      return parsedValue;
    }

    private double EvaluateFormula(string formulaText, double pointValue)
    {
      string preparedFormula = Regex.Replace(
        formulaText,
        @"([\w\.]+)\s*\^\s*([\w\.]+)",
        "Pow($1, $2)");

      NCalc.Expression expression = new NCalc.Expression(preparedFormula);
      expression.Parameters["x"] = pointValue;

      double resultValue = Convert.ToDouble(expression.Evaluate());
      return resultValue;
    }

    private List<Tuple<double, double>> FindSignChanges(string formulaText, double startValue, double endValue)
    {
      List<Tuple<double, double>> foundIntervals = new List<Tuple<double, double>>();

      double stepValue = (endValue - startValue) / signCheckSteps;
      double previousPoint = startValue;
      double previousValue = EvaluateFormula(formulaText, startValue);

      for (int stepIndex = 1; stepIndex <= signCheckSteps; stepIndex++)
      {
        double currentPoint = startValue + stepIndex * stepValue;
        double currentValue = EvaluateFormula(formulaText, currentPoint);

        if (previousValue == 0.0)
        {
          foundIntervals.Add(Tuple.Create(previousPoint, previousPoint));
        }
        else if (previousValue * currentValue < 0.0)
        {
          foundIntervals.Add(Tuple.Create(previousPoint, currentPoint));
        }

        previousPoint = currentPoint;
        previousValue = currentValue;
      }

      return foundIntervals;
    }

    private double FindRootByDichotomy(string formulaText, double startValue, double endValue, double accuracyValue, out int iterationCount)
    {
      iterationCount = 0;

      double startFunctionValue = EvaluateFormula(formulaText, startValue);
      double endFunctionValue = EvaluateFormula(formulaText, endValue);

      if (startFunctionValue * endFunctionValue > 0.0)
      {
        throw new Exception("Function has same signs on interval ends - no root.");
      }

      double middlePoint = startValue;

      while ((endValue - startValue) / 2.0 > accuracyValue)
      {
        middlePoint = (startValue + endValue) / 2.0;
        double middleFunctionValue = EvaluateFormula(formulaText, middlePoint);

        if (startFunctionValue * middleFunctionValue < 0.0)
        {
          endValue = middlePoint;
          endFunctionValue = middleFunctionValue;
        }
        else
        {
          startValue = middlePoint;
          startFunctionValue = middleFunctionValue;
        }

        iterationCount++;
      }

      double rootValue = (startValue + endValue) / 2.0;
      return rootValue;
    }

    private void OnCalculateClick(object? sender, EventArgs e)
    {
      try
      {
        double startValue = ParseDoubleValue(leftBoundBox.Text);
        double endValue = ParseDoubleValue(rightBoundBox.Text);
        double accuracyValue = ParseDoubleValue(accuracyBox.Text);
        string formulaText = formulaBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(formulaText))
        {
          throw new Exception("Enter formula f(x).");
        }

        if (accuracyValue <= 0.0)
        {
          throw new Exception("Accuracy e must be greater than 0.");
        }

        if (startValue >= endValue)
        {
          throw new Exception("Condition a < b must be satisfied.");
        }

        try
        {
          EvaluateFormula(formulaText, startValue);
          EvaluateFormula(formulaText, endValue);
        }
        catch
        {
          throw new Exception("Invalid formula f(x).");
        }

        List<Tuple<double, double>> signChangeIntervals = FindSignChanges(formulaText, startValue, endValue);

        if (signChangeIntervals.Count == 0)
        {
          throw new Exception("No roots found on the given interval.");
        }

        if (signChangeIntervals.Count > 1)
        {
          throw new Exception("Several roots on the selected interval. Specify [a, b] more precisely.");
        }

        int iterationCount = 0;
        double rootValue = FindRootByDichotomy(formulaText, startValue, endValue, accuracyValue, out iterationCount);
        double rootFunctionValue = EvaluateFormula(formulaText, rootValue);

        resultLabel.Text = "Root: x = " + rootValue.ToString("F6") +
                           "   f(x) = " + rootFunctionValue.ToString("E3") +
                           "   Iterations: " + iterationCount.ToString();

        DrawGraph(formulaText, startValue, endValue, rootValue);
      }
      catch (Exception exception)
      {
        MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void OnPlotClick(object? sender, EventArgs e)
    {
      try
      {
        double startValue = ParseDoubleValue(leftBoundBox.Text);
        double endValue = ParseDoubleValue(rightBoundBox.Text);
        string formulaText = formulaBox.Text.Trim();

        DrawGraph(formulaText, startValue, endValue, null);
      }
      catch (Exception exception)
      {
        MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void OnClearClick(object? sender, EventArgs e)
    {
      leftBoundBox.Text = "0";
      rightBoundBox.Text = "3";
      accuracyBox.Text = "0.0001";
      formulaBox.Text = "x^2 + 2*x - 6";
      resultLabel.Text = "Enter data and press Calculate";
      graphBox.Image = null;
    }

    private void DrawGraph(string formulaText, double startValue, double endValue, double? rootValue)
    {
      Bitmap graphBitmap = new Bitmap(graphBox.Width, graphBox.Height);

      using (Graphics graphics = Graphics.FromImage(graphBitmap))
      {
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(Color.White);

        int widthValue = graphBitmap.Width;
        int heightValue = graphBitmap.Height;

        double minimumY = double.MaxValue;
        double maximumY = double.MinValue;

        double[] pointArray = new double[graphSteps + 1];
        double[] valueArray = new double[graphSteps + 1];

        for (int stepIndex = 0; stepIndex <= graphSteps; stepIndex++)
        {
          double currentPoint = startValue + (endValue - startValue) * stepIndex / graphSteps;
          pointArray[stepIndex] = currentPoint;

          try
          {
            double currentValue = EvaluateFormula(formulaText, currentPoint);

            if (double.IsNaN(currentValue) || double.IsInfinity(currentValue))
            {
              currentValue = 0.0;
            }

            valueArray[stepIndex] = currentValue;

            if (currentValue < minimumY)
            {
              minimumY = currentValue;
            }

            if (currentValue > maximumY)
            {
              maximumY = currentValue;
            }
          }
          catch
          {
            valueArray[stepIndex] = 0.0;
          }
        }

        if (maximumY - minimumY < 1e-9)
        {
          maximumY += 1.0;
          minimumY -= 1.0;
        }

        double paddingValue = (maximumY - minimumY) * 0.1;
        minimumY -= paddingValue;
        maximumY += paddingValue;

        Func<double, float> toPixelX = point => (float)(marginSize + (point - startValue) / (endValue - startValue) * (widthValue - 2 * marginSize));
        Func<double, float> toPixelY = value => (float)(heightValue - marginSize - (value - minimumY) / (maximumY - minimumY) * (heightValue - 2 * marginSize));

        Pen axisPen = new Pen(Color.Gray, 1.0f);

        float zeroLineY = toPixelY(0.0);

        if (zeroLineY > marginSize && zeroLineY < heightValue - marginSize)
        {
          graphics.DrawLine(axisPen, marginSize, zeroLineY, widthValue - marginSize, zeroLineY);
        }

        float zeroLineX = toPixelX(0.0);

        if (zeroLineX > marginSize && zeroLineX < widthValue - marginSize)
        {
          graphics.DrawLine(axisPen, zeroLineX, marginSize, zeroLineX, heightValue - marginSize);
        }

        graphics.DrawString("x", new Font("Arial", 10.0f), Brushes.Black, widthValue - marginSize + 5, zeroLineY - 15);
        graphics.DrawString("y", new Font("Arial", 10.0f), Brushes.Black, zeroLineX + 5, marginSize - 15);

        Pen curvePen = new Pen(Color.Blue, 2.0f);

        for (int stepIndex = 0; stepIndex < graphSteps; stepIndex++)
        {
          float firstPixelX = toPixelX(pointArray[stepIndex]);
          float firstPixelY = toPixelY(valueArray[stepIndex]);
          float secondPixelX = toPixelX(pointArray[stepIndex + 1]);
          float secondPixelY = toPixelY(valueArray[stepIndex + 1]);

          if (Math.Abs(firstPixelY - secondPixelY) > heightValue * 2)
          {
            continue;
          }

          graphics.DrawLine(curvePen, firstPixelX, firstPixelY, secondPixelX, secondPixelY);
        }

        if (rootValue.HasValue)
        {
          double actualRoot = rootValue.Value;
          float rootPixelX = toPixelX(actualRoot);
          float rootPixelY = toPixelY(0.0);

          graphics.FillEllipse(Brushes.Red, rootPixelX - 5, rootPixelY - 5, 10, 10);
          graphics.DrawString("x = " + actualRoot.ToString("F4"),
                              new Font("Arial", 9.0f, FontStyle.Bold),
                              Brushes.Red,
                              rootPixelX + 8,
                              rootPixelY - 20);
        }

        graphics.DrawRectangle(Pens.Black, marginSize, marginSize, widthValue - 2 * marginSize, heightValue - 2 * marginSize);
      }

      graphBox.Image = graphBitmap;
    }
  }
}