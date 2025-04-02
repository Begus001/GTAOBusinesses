using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GTAOBusinesses
{
    /// <summary>
    /// Interaction logic for CayoCalc.xaml
    /// </summary>
    public partial class CayoCalc : Window
    {
		private int[] nums = new int[3];
		private int[] muls = new int[3];
		private int target = 1;
		public CayoCalc()
		{
			InitializeComponent();
			tbTarget.Focus();
			tbTarget.SelectAll();
		}
		private bool TrySolve(out (int num, int mul)[] solution)
		{
			solution = null;
			var permutations = new int[][] {
				new int[] { 0, 1, 2 },
				new int[] { 0, 2, 1 },
				new int[] { 1, 0, 2 },
				new int[] { 1, 2, 0 },
				new int[] { 2, 0, 1 },
				new int[] { 2, 1, 0 }
			};

			foreach (var perm in permutations)
			{
				int sum = nums[perm[0]] * muls[0] + nums[perm[1]] * muls[1] + nums[perm[2]] * muls[2];
				Debug.WriteLine($"{sum} = {nums[perm[0]]} * {muls[0]} + {nums[perm[1]]} * {muls[1]} + {nums[perm[2]]} * {muls[2]}");
				if (sum == target)
				{
					solution = new (int, int)[] {
						(nums[perm[0]], muls[0]),
						(nums[perm[1]], muls[1]),
						(nums[perm[2]], muls[2])
					};
					return true;
				}
			}
			return false;
		}

		private int validate(TextBox tb, bool check = true)
		{
			try
			{
				int val = Convert.ToInt32(tb.Text);
				if (check && (val < 0 || val > 9))
				{
					tb.BorderBrush = Brushes.MediumVioletRed;
					throw new Exception();
				}
				tb.ClearValue(BorderBrushProperty);
				return val;
			}
			catch
			{
				tb.BorderBrush = Brushes.MediumVioletRed;
				throw new Exception();
			}
		}

		private void inputChanged(object sender, EventArgs e)
		{
			try
			{
				if (sender is TextBox)
				{
					if ((e as KeyEventArgs).Key == Key.Tab)
					{
						(sender as TextBox).SelectAll();
					}
				}
				nums[0] = validate(tbNum1);
				nums[1] = validate(tbNum2);
				nums[2] = validate(tbNum3);
				target = validate(tbTarget, false);
				muls[0] = Convert.ToInt32(cbMul1.Text.Substring(1));
				muls[1] = Convert.ToInt32(cbMul2.Text.Substring(1));
				muls[2] = Convert.ToInt32(cbMul3.Text.Substring(1));
				if (TrySolve(out var solution))
				{
					lbResult.Content = $"Solution:    {solution[0].num}: x{solution[0].mul}    {solution[1].num}: x{solution[1].mul}    {solution[2].num}: x{solution[2].mul}";

				}
				else
				{
					lbResult.Content = "No solution";
				}
			}
			catch
			{
				if (lbResult != null)
					lbResult.Content = "Input not valid";
			}
		}
	}
}
