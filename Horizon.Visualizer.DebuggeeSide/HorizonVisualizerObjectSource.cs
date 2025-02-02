﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.IO;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace Allegro.Visualizer.NetCore.DebuggeeSide
{
    public class HorizonVisualizerObjectSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            if (target != null)
            {
                if (target is DataRow row)
                {
                    DataTable table;

                    if (row.Table == null)
                    {
                        table = new DataTable();

                        for (int i = 0; i < row.ItemArray.Length; i++)
                        {
                            table.Columns.Add(string.Format("Col{0}", i.ToString()), typeof(string));
                        }
                    }
                    else
                    {
                        table = row.Table.Clone();
                    }
                    table.Constraints.Clear();
                    table.LoadDataRow(row.ItemArray, true);

                    StreamSerializer.ObjectToStream(outgoingData, table);
                }               
                else if (target is DataView view)
                {
                    DataTable table = view.Table.Clone();
                    table.Constraints.Clear();

                    foreach (DataRow dataRow in view.Table.Rows)
                    {
                        table.LoadDataRow(dataRow.ItemArray, true);
                    }

                    StreamSerializer.ObjectToStream(outgoingData, table);
                }
                else if (target is DataRowView dataRowView)
                {
                    DataTable table = dataRowView.Row.Table.Clone();
                    table.Constraints.Clear();
                    table.LoadDataRow(dataRowView.Row.ItemArray, true);

                    StreamSerializer.ObjectToStream(outgoingData, table);
                }
                else if (target is DataRowCollection dataRowCollection)
                {
                    DataTable table = null;

                    if (dataRowCollection != null && dataRowCollection.Count > 0)
                    {
                        if (dataRowCollection[0].Table == null)
                        {
                            table = new DataTable();

                            for (int i = 0; i < dataRowCollection[0].ItemArray.Length; i++)
                            {
                                table.Columns.Add(string.Format("Col{0}", i.ToString()), typeof(string));
                            }
                        }
                        else
                        {
                            table = dataRowCollection[0].Table.Clone();
                        }
                        table.Constraints.Clear();

                        foreach (DataRow dataRow in dataRowCollection)
                        {
                            table.LoadDataRow(dataRow.ItemArray, true);
                        }
                    }

                    StreamSerializer.ObjectToStream(outgoingData, table);
                }
                else if (target is DataTable dataTable)
                {
                    dataTable.Constraints.Clear();
                    StreamSerializer.ObjectToStream(outgoingData, dataTable);
                }
                else if (target is DataSet dataSet)
                {
                    dataSet.EnforceConstraints = false;
                    StreamSerializer.ObjectToStream(outgoingData, dataSet);
                }
                else if (target is DataRow[] rows)
                {
                    DataTable dt = rows[0].Table.Clone();
                    dt.Constraints.Clear();
                    foreach (DataRow dr in rows)
                    {
                        dt.ImportRow(dr);
                    }
                    dt.AcceptChanges();
                    StreamSerializer.ObjectToStream(outgoingData, dt);
                }
                else if (target is IList list)
                {
                    var table = ToDataTable(list);
                    StreamSerializer.ObjectToStream(outgoingData, table);
                }
            }
        }

        public DataTable ToDataTable(IList data)
        {
            Type type = data.GetType().GenericTypeArguments[0];
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(type);
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (var item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
