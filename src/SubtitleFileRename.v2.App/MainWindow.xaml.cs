using SubtitleFileRename.Services.Models;
using SubtitleFileRename.v2.App.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SubtitleFileRename.v2.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            DataContext = viewModel;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(MainWindowViewModel.Data))
            {
                if (sender is MainWindowViewModel viewModel)
                {
                    if (viewModel.Data != null && viewModel.Data.Files != null)
                    {
                        ClearAddedRows();

                        var item = viewModel.Data.Files.Where(x => x.Key.IsPrimary).FirstOrDefault();
                        if (item.Key != null)
                        {
                            var addedIndex = AddRowOnMainLayout();

                            AddListView(addedIndex, item.Value);
                        }

                        foreach(var anotheritem in viewModel.Data.Files.Where(x => !x.Key.IsPrimary))
                        {
                            if (anotheritem.Key != null)
                            {
                                var addedIndex = AddRowOnMainLayout();

                                AddListView(addedIndex, anotheritem.Value);
                            }
                        }
                    }
                }
            }
        }


        private void AddRowsOnMainLayout(int howManyRows)
        {
            for (var i = 0; i < howManyRows; i++)
            {
                AddRowOnMainLayout();
            }
        }

        /// <summary>
        /// Add row on Main grid layout.
        /// </summary>
        /// <returns>Added row index</returns>
        private int AddRowOnMainLayout()
        {
            var rowDefinition = new RowDefinition();
            var height = new GridLength(1, GridUnitType.Star);

            rowDefinition.Height = height;
            mainLayoutGrid.RowDefinitions.Add(rowDefinition);

            return mainLayoutGrid.RowDefinitions.Count - 1;
        }

        private void ClearAddedRows()
        {
            if(mainLayoutGrid.RowDefinitions.Count > 3)
            {
                var count = mainLayoutGrid.RowDefinitions.Count;
                for (var i = count - 1; i >= 3; i--)
                {
                    mainLayoutGrid.Children.RemoveAt(i);
                    mainLayoutGrid.RowDefinitions.RemoveAt(i);
                }
            }
        }

        private void AddListView(int rowIndex, IEnumerable<TargetFile> data)
        {
            var listView = new ListView();
            var gridView = new GridView();
            gridView.Columns.Add(new GridViewColumn
            {
                DisplayMemberBinding = new Binding(nameof(TargetFile.Name)),
                Header = "File name",
                Width =  double.NaN,
            });
            gridView.Columns.Add(new GridViewColumn
            {
                DisplayMemberBinding = new Binding(nameof(TargetFile.Extension)),
                Header = "Ext",
                Width = double.NaN,
            });
            gridView.Columns.Add(new GridViewColumn
            {
                DisplayMemberBinding = new Binding(nameof(TargetFile.Path)),
                Header = "Path",
                Width = double.NaN,
            });
            gridView.Columns.Add(new GridViewColumn
            {
                DisplayMemberBinding = new Binding(nameof(TargetFile.CandidateName)),
                Header = "Rename candidate",
                Width = double.NaN,
            });
            gridView.Columns.Add(new GridViewColumn
            {
                DisplayMemberBinding = new Binding(nameof(TargetFile.OriginalName)),
                Header = "Original name",
                Width = double.NaN,
            });
            gridView.Columns.Add(new GridViewColumn
            {
                DisplayMemberBinding = new Binding(nameof(TargetFile.Message)),
                Header = "Message",
                Width = double.NaN,
            });
            listView.View = gridView;

            Grid.SetRow(listView, rowIndex);
            mainLayoutGrid.Children.Add(listView);

            listView.ItemsSource = data;
        }
    }
}
