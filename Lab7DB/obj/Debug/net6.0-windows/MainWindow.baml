   M S B A M L   `   `   `  ����A  =Lab7DB, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:Lab7DB.ViewModelLab7DB.ViewModel  5   	       Lab7DB.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:Lab7DB.ViewModel     5   	     � Title$  
MainWindow��5   	   $	��450��6   $
��1000��6)   $	��350��66   $
��1000��6F    � DataContext 5	      #   Lab7DB.ViewModel.VMMainWindow  5
   
   .��5      �  � RowDefinitions 5   
   �� 5      $��30 �6   �� 5      $��20 �6   �� 5      �� 5      �� 5      $��70 �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $
��1.5* �6   �� 5      $
��0.5* �6   .+�5   
   o� $��0��6   $��3��6   $��0��63   .@�5      m� $��Создать��6   $��30��6(    m� Command 67   �� *CreateTable+m� 5      $��Открыть��6   $��30��6(    67   �� *
AddFiles+m� 5      $��Очистить��6   $��30��6)    68   �� *Clear+m� 5      $��Обновить��6   $��30��6)    68   �� *Update+m� 5      $!��Создать запрос��6   $��30��6/    6>   �� *RequestWindow+5       R� 5!   
   $��1��6   $��3��6    $��0��61   ��6A   �� *TreeElement+��5"      �� 5#       �� ItemsSource 6+   �� *
Children+.R�5$      �� ��6    �� *Name+5%      5'      �� 5(   
   $��1��6   $��1��6    ��60   �� *CollectionNameTables+ �� SelectedItem 6]   �� *SelectedNameTable+&   System.Windows.Controls.DataGrid  5)   
   $��2��6   $��3��6    $��1��61   $��1��6A      
IsReadOnly$
 True��6{   "   VerticalScrollBarVisibility$ Visible=�6�   $	   HorizontalScrollBarVisibility$	 Visible=�6�   ��6U   �� *SelectedTable+6�   o� 5*   
   $��1��6   $��2��6   $��2��6-   .@�5+      m� $#��Вывести таблицу��6    60   �� *OutputTable+m� 5,      $/��)Редактировать таблицу��6    66   �� *RewriteTable+m� 5-      $-��'Редактировать данные��6    65   �� *RewriteData+5.      �� 5/   
   $��4��6   $��0��6    $��Окно ошибок��60   .��50      �� $
��Wrap=�6<   ��6   �� *ContentErrorWindow+6<   51      � 52   
   $��2��6   $��3��6   $��2��6,   $��2q�6=    53      �� 54      $��20 �6    �� 55      $��10 �6    �� 56      $��20 �6    �� 57      �� 58      $��20 �6    �� 59      $��20 �6     5;      �� 5<      �� 5=      $
��0.5* �6#   .+�5@      �� $��0��6   $��2��6(   $��0��6<   $2��,Просмотр внешних ключей��6I   �� 5A      $��0��6   $��2��6'   ��64   �� *NameColumnWithForeignKey+ 6e   �� *" SelectedNameColumnWithForeignKey+�� 5B      $��1��6   $��2��6%   $��Выбрать��62   ��6D   �� *OutPutForeignTable+  5C      $��0��6   $��2��6'   $��3��6;   ��6H   �� *TableForeing+�� 5D      $��0��6   $��2��6(   $��4��6<   $��Строки��6I   �� 5E      $��0��6   $��5��6'   ��64   �� *CellsForeingColumn+ 6_   �� *SelectedCellsForeing+�� 5F      $��1��6   $��5��6%   $��Выбрать��62   ��6D   �� *OutPutForeignLine+5G      5H      5I      