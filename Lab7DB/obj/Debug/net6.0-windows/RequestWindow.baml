   M S B A M L   `   `   `  ����A  =Lab7DB, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:Lab7DBLab7DB  5   	       Lab7DB.RequestWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:Lab7DB     5   	     � Title$  RequestWindow��5   	   $	��450��6   $	��800��6,   .��5	      � $��10q�6    � RowDefinitions 5
   
   �� 5      �� 5      �� 5      $��0.10* �6    � ColumnDefinitions 5   
   �� 5      �� 5      $��0.25* �6   �� 5      $��0.20* �6   .+�5   
   &   System.Windows.Controls.DataGrid  $��0��6   $��0��6#   ��60   �� *TableOutPut+�� 5   
   $��1��6   $��2��6#   $��0��67   $��Окно ошибок��6D   .��5      �� $
��Wrap=�61   ��6   �� *	Content+61   5      �� 5   
   $��0��6   $��3��6"   $��1��66    �� TextWrapping$
 Wrap=�6`    �� AcceptsReturn$
 True��6t   " �� VerticalScrollBarVisibility$ Visible=�6�   ��6C   �� *WindowInPut+6�   �� 5   
   $��2��6   $��2��6!   $��Выполнить��6.   ��6B   �� *	Execute+5      5      