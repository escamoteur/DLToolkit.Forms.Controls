﻿using System;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

namespace DLToolkit.Forms.Controls
{
	/// <summary>
	/// Flow list view internal cell.
	/// </summary>
	public class FlowListViewInternalCell : ViewCell
	{
		readonly WeakReference<FlowListView> _flowListViewRef;
		readonly AbsoluteLayout _rootLayout;
		//int? _flowColumnCount;
		int _desiredColumnCount;
		DataTemplate _flowColumnTemplate;
		FlowColumnExpand _flowColumnExpand;
		IList<DataTemplate> _currentColumnTemplates = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="DLToolkit.Forms.Controls.FlowListViewInternalCell"/> class.
		/// </summary>
		/// <param name="flowListViewRef">Flow list view reference.</param>
		public FlowListViewInternalCell(WeakReference<FlowListView> flowListViewRef)
		{
			_flowListViewRef = flowListViewRef;
			FlowListView flowListView = null;
			flowListViewRef.TryGetTarget(out flowListView);

			_rootLayout = new AbsoluteLayout() {
				Padding = 0d,
				BackgroundColor = flowListView.FlowRowBackgroundColor,
			};

			View = _rootLayout;

			_flowColumnTemplate = flowListView.FlowColumnTemplate;
			_desiredColumnCount = flowListView.DesiredColumnCount;
			//_flowColumnCount = flowListView.FlowColumnCount;
			_flowColumnExpand = flowListView.FlowColumnExpand;
		}

		private IList<DataTemplate> GetDataTemplates(IList container)
		{
			List<DataTemplate> templates = new List<DataTemplate>();

			var templateSelector = _flowColumnTemplate as DataTemplateSelector;

			if (templateSelector != null)
			{
				for (int i = 0; i < container.Count; i++)
				{
					var template = templateSelector.SelectTemplate(container[i], null);
					templates.Add(template);
				}
			}
			else
			{
				for (int i = 0; i < container.Count; i++)
				{
					templates.Add(_flowColumnTemplate);
				}
			}

			return templates;
		}

		private bool RowLayoutChanged(int containerCount, IList<DataTemplate> templates)
		{
			// Check if desired number of columns is equal to current number of columns
			if (_currentColumnTemplates == null || containerCount != _currentColumnTemplates.Count)
			{
				return true;
			}

			// Check if desired column view types are equal to current columns view types
			for (int i = 0; i < containerCount; i++)
			{
				if (_currentColumnTemplates[i].GetType() != templates[i].GetType())
				{
					return true;
				}
			}

			return false;
		}

		private void SetBindingContextForView(View view, object bindingContext)
		{
			if (view != null && view.BindingContext != bindingContext)
				view.BindingContext = bindingContext;
		}

		private void AddViewToLayout(View view, int containerCount, int colNumber)
		{
			if (containerCount == 0 || _desiredColumnCount == 0)
				return;

			double desiredColumnWidth = 1d / _desiredColumnCount;

            Rectangle bounds = Rectangle.Zero;

			if (_flowColumnExpand != FlowColumnExpand.None && _desiredColumnCount > containerCount)
			{
				int diff = _desiredColumnCount - containerCount;
				bool isLastColumn = colNumber == containerCount - 1;

				switch (_flowColumnExpand)
				{
					case FlowColumnExpand.First:

						if (colNumber == 0)
						{
							bounds = new Rectangle(0d, 0d, desiredColumnWidth + (desiredColumnWidth * diff), 1d);
						}
						else if (isLastColumn)
						{
							bounds = new Rectangle(1d, 0d, desiredColumnWidth, 1d);
						}
						else
						{
							bounds = new Rectangle(desiredColumnWidth * (colNumber + diff) / (1d - desiredColumnWidth), 0d, desiredColumnWidth, 1d);
						}

						break;

					case FlowColumnExpand.Last:

						if (colNumber == 0)
						{
							bounds = new Rectangle(0d, 0d, desiredColumnWidth + (desiredColumnWidth * diff), 1d);
						}
						else if (isLastColumn)
						{
							bounds = new Rectangle(1d, 0d, desiredColumnWidth + (desiredColumnWidth * diff), 1d);
						}
						else
						{
							bounds = new Rectangle(desiredColumnWidth * colNumber / (1d - desiredColumnWidth), 0d, desiredColumnWidth, 1d);
						}

						break;

					case FlowColumnExpand.Proportional:
						
						double propColumnsWidth = 1d / containerCount;
						if (colNumber == 0)
						{
							bounds = new Rectangle(0d, 0d, propColumnsWidth, 1d);	
						}
						else if (isLastColumn)
						{
							bounds = new Rectangle(1d, 0d, propColumnsWidth, 1d);	
						}
						else
						{
							bounds = new Rectangle(propColumnsWidth * colNumber / (1d - propColumnsWidth), 0d, propColumnsWidth, 1d);	
						}

						break;

					case FlowColumnExpand.ProportionalFirst:
						
						int propFMod = _desiredColumnCount % containerCount;
						double propFSize = desiredColumnWidth * Math.Floor((double)_desiredColumnCount / containerCount);
						double propFSizeFirst = propFSize + desiredColumnWidth * propFMod;

						if (colNumber == 0)
						{
							bounds = new Rectangle(0d, 0d, propFSizeFirst, 1d);
						}
						else if (isLastColumn)
						{
							bounds = new Rectangle(1d, 0d, propFSize, 1d);
						}
						else
						{
							bounds = new Rectangle(((propFSize * colNumber) + (propFSizeFirst - propFSize)) / (1d - propFSize), 0d, propFSize, 1d);	
						}

						break;

					case FlowColumnExpand.ProportionalLast:

						int propLMod = _desiredColumnCount % containerCount;
						double propLSize = desiredColumnWidth * Math.Floor((double)_desiredColumnCount / containerCount);
						double propLSizeLast = propLSize + desiredColumnWidth * propLMod;

						if (colNumber == 0)
						{
							bounds = new Rectangle(0d, 0d, propLSize, 1d);
						}
						else if (isLastColumn)
						{
							bounds = new Rectangle(1d, 0d, propLSizeLast, 1d);
						}
						else
						{
							bounds = new Rectangle((propLSize * colNumber) / (1d - propLSize), 0d, propLSize, 1d);	
						}

						break;
				}		
			}
			else
			{
				if (Math.Abs(1d - desiredColumnWidth) < Epsilon.DoubleValue)
				{
					bounds = new Rectangle(1d, 0d, desiredColumnWidth, 1d);	
				}
				else
				{
					bounds = new Rectangle(desiredColumnWidth * colNumber / (1d - desiredColumnWidth), 0d, desiredColumnWidth, 1d);	
				}
			}

            _rootLayout.Children.Add(view, bounds, AbsoluteLayoutFlags.All);
		}

		/// <summary>
		/// Override this method to execute an action when the BindingContext changes.
		/// </summary>
		/// <remarks></remarks>
		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			var container = BindingContext as IList;

			if (container == null)
				return;

			FlowListView flowListView = null;
			if (_flowListViewRef.TryGetTarget(out flowListView) && flowListView != null)
			{
				_flowColumnTemplate = flowListView.FlowColumnTemplate;
				_desiredColumnCount = flowListView.DesiredColumnCount;
				//_flowColumnCount = flowListView.FlowColumnCount;
				_flowColumnExpand = flowListView.FlowColumnExpand;
			}
				
			// Getting view types from templates
			var containerCount = container.Count;
			IList<DataTemplate> templates = GetDataTemplates(container);
			bool layoutChanged = RowLayoutChanged(containerCount, templates);

			if (!layoutChanged) // REUSE VIEWS
			{
				for (int i = 0; i < containerCount; i++)
				{
					SetBindingContextForView(_rootLayout.Children[i], container[i]);
				}
			}
			else // RECREATE COLUMNS
			{
				if (_rootLayout.Children.Count > 0)
					_rootLayout.Children.Clear();

				_currentColumnTemplates = new List<DataTemplate>(templates);

				for (int i = 0; i < containerCount; i++)
				{
					var view = (View)templates[i].CreateContent();

					view.GestureRecognizers.Add(new TapGestureRecognizer()
					{
						Command = new Command(async (obj) =>
						{
							await ExecuteTapGestureRecognizer(view);
						})
					});

					SetBindingContextForView(view, container[i]);
					AddViewToLayout(view, containerCount, i);
				}
			}
		}

		async Task ExecuteTapGestureRecognizer(View view)
		{
			var flowCell = view as IFlowViewCell;
			if (flowCell != null)
			{
				flowCell.OnTapped();
			}

			FlowListView flowListView = null;
			_flowListViewRef.TryGetTarget(out flowListView);

			if (flowListView != null)
			{
				int tapBackgroundEffectDelay = flowListView.FlowTappedBackgroundDelay;

				try
				{
					if (tapBackgroundEffectDelay != 0)
					{
						view.BackgroundColor = flowListView.FlowTappedBackgroundColor;
					}

					flowListView.FlowPerformTap(view.BindingContext);
				}
				finally
				{
					if (tapBackgroundEffectDelay != 0)
					{
						await Task.Delay(tapBackgroundEffectDelay);
						view.BackgroundColor = flowListView.FlowRowBackgroundColor;
					}
				}
			}
		}
}
}

