using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net.NetworkInformation;

namespace megamot11;
enum ReportType
{
    Collect,
    Analyze,
    Recon,
    Intel
}

enum Status
{
    Pending,
    Approved,
    Rejected
}

class ReportAnalyzer
{
    //check if file exists and opens file to read it
    //if file does not exists returns null
    static string[]? LoadFile(string path, int arraySize)
    {
        if (!File.Exists(path))
        {
            return null;
        }
        string[] data = File.ReadAllLines(path);

        if (data.Length == 0)
        {
            Console.WriteLine("Error: File is empty.");
        }
        else
        {
            Console.WriteLine($"File loaded: {data.Length} lines found");
        }


        return data;
    }

    //split each line in to array of strings
    static string[][] splitTheData(string[] data)
    {
        string[][] splitLine= new string[data.Length][];
        for (int i = 0; i < data.Length; i++)
        {
            splitLine[i] = data[i].Split(',');
        }
        return splitLine;
    }


    //validate data - helper function (helped by smaller funcions)
    static bool validateData(string[] line)
    {
        if (line.Length != 5)
        {
            return false;
        }

        return validateUnit(line[0]) && validateReport(line[1]) && validatePriority(line[2]) && validateScore(line[3]) && validateStatus(line[4]);
    }


    //validate unit - helper function
    static bool validateUnit(string unit)
    {
        unit = unit.Trim();
        if(unit == "")
        {
            return false;
        }
        return true;
    }


    //validate report - helper function
    static bool validateReport(string report)
    {
        report = report.Trim();
        if (!Enum.TryParse<ReportType>(report, true, out ReportType reportInEnum))
        {
            return false;
        }
        return true;
    }


    //validate status - helper function
    static bool validateStatus(string status)
    {
        status = status.Trim();
        if (!Enum.TryParse<Status>(status, true, out Status statusInEnum))
        {
            return false;
        }
        return true;
    }

    //validate priority - helper function
    static bool validatePriority(string priority)
    {
        priority = priority.Trim();
        if (!(int.TryParse(priority, out int intPriority) && intPriority >=1 && intPriority <=5))
        {
            return false;
        }

        return true;
    }

    //validate score - helper function
    static bool validateScore(string score)
    {
        score = score.Trim();
        if (!(double.TryParse(score, out double doubleScore) && doubleScore >= 0.0 && doubleScore <=100.0))
        {
            return false;
        }
        return true;
    }

    //parse line[0] in to ReportType
    static ReportType getEnumReportType(string rt)
    {
        rt = rt.Trim();
        ReportType reportTypeInEnum;
        Enum.TryParse<ReportType>(rt, true, out reportTypeInEnum);
        return reportTypeInEnum;
    }


    //parse line[1] in to int
    static int getIntPriority(string pr)
    {
        return int.Parse(pr);
    }


    //parse line[2] in to double
    static double getDoubleScore(string sc)
    {
        return double.Parse(sc);
    }


    //parse line[4] in to Status
    static Status getEnumStatus(string st)
    {
        st = st.Trim();
        Status statusInEnum;
        Enum.TryParse<Status>(st, true, out statusInEnum);
        return statusInEnum;
    }


    //add each part of the line to its string. in the right index
    static bool addLineToStatisticsDb(int index, string[] line, string[] unitName, ReportType[] reportType, int[] priority, double[] score, Status[] status)
    {
        if (validateData(line))
        {
            unitName[index] = line[0].Trim();
            reportType[index] = getEnumReportType(line[1]);
            priority[index] = getIntPriority(line[2]);
            score[index] = getDoubleScore (line[3]);
            status[index] = getEnumStatus(line[4]);
            return true;
        }
        return false;
    }

    //manage all processing. validation and the actual process. returns number of validated processed lines
    static int ProcessReports(string[] unitName, ReportType[] reportType, int[] priority, double[] score, Status[] status,string[] data)
    {
        string[][] spliteLines = splitTheData(data);
        int count = 0;
        foreach (string[] line in spliteLines)
        {
            bool did_add = addLineToStatisticsDb(count, line, unitName, reportType, priority, score, status);
            if (did_add)
            {
                count++;
            }
        }
        return count;
    }


    //calculate avarage score
    static string CalculateAverage(double[] score, int numOfReports)
    {
        double sum = 0;
        for(int i = 0; i < numOfReports; i++)
        {
            sum += score[i];
        }
        return (numOfReports > 0) ? (sum / numOfReports).ToString("F2") : "No Reports.";
    }


    //find max score
    static string FindMaxScore(double[] score, int numOfReports)
    {
        double maxScore = 0;
        for (int i = 0; i < numOfReports; i++)
        {
            if (score[i] > maxScore)
            {
                maxScore = score[i];
            }
        }
        return (numOfReports > 0) ? maxScore.ToString("F2") : "No Reports.";
    }


    //find min score
    static string FindMinScore(double[] score, int numOfReports)
    {
        double minScore = 100;
        for (int i = 0; i < numOfReports; i++)
        {
            if (score[i] < minScore)
            {
                minScore = score[i];
            }
        }
        return (numOfReports > 0) ? minScore.ToString("F2") : "No Reports.";
    }


    //count how many by status
    static int CountByStatus(Status stauts, Status[] statusArray, int numOfReports)
    {
        int count = 0;
        for (int i = 0; i < numOfReports; i++)
        {
            if (statusArray[i] == stauts)
            {
                count++;
            }
        }
        return count;
    }


    //display basic stats
    static void DisplayBasicStatistics(double[] score, int numOfReports)
    {
        Console.WriteLine($"""

            ===Display Basic Stats===
            Basic Status reports:
            Average Score: {CalculateAverage(score, numOfReports):F2}
            Max Score: {FindMaxScore(score, numOfReports)}
            Min Score: {FindMinScore(score, numOfReports)}

            """);
    }

    //display status counts
    static void DisplayStatusCounts(Status[] statusArray, int numOfReports)
    {
        Console.WriteLine($"""
            ===Display Count Status===
            Count Status Reports:
            Status Approved: {CountByStatus(Status.Approved, statusArray, numOfReports)}
            Status Pending: {CountByStatus(Status.Pending, statusArray, numOfReports)}
            Status Rejected: {CountByStatus(Status.Rejected, statusArray, numOfReports)}

            """);
    }


    //display report type counts
    static void DisplayTypeCounts(ReportType[] reportTypeArray, int numOfReports)
    {
        Console.WriteLine($"""
            ===Display Count Report Types===
            Report Type reports:
            Report Type Approved: {CountByType(ReportType.Analyze, reportTypeArray, numOfReports)}
            Report Type Pending: {CountByType(ReportType.Collect, reportTypeArray, numOfReports)}
            Report Type Rejected: {CountByType(ReportType.Intel, reportTypeArray, numOfReports)}
            Report Type Rejected: {CountByType(ReportType.Recon, reportTypeArray, numOfReports)}

            """);
    }


    //count how many by type
    static int CountByType(ReportType reporttype, ReportType[] reportTypes, int numOfReports)
    {
        int count = 0;
        for (int i = 0; i < numOfReports; i++)
        {
            if (reportTypes[i] == reporttype)
            {
                count++;
            }
        }
        return count;
    }


    //display hgiest score with approved status
    static void DisplayHighestPriorityApproved(string[] unitName, ReportType[] reportType, int[] priority, double[] score, Status[] status, int numOfReports)
    {
        int maxPriorityindex = -1;
        Console.WriteLine($"===Display Approved status And Highes Priority===");
        for (int i = 0; i < numOfReports; i++)
        {
            if (status[i] == Status.Approved)
            {
               if (maxPriorityindex == -1 || priority[i] >= priority[maxPriorityindex])
                {
                    maxPriorityindex = i;
                }
            }
        }
        if (maxPriorityindex != -1)
        { Console.WriteLine($"""
                Unit Name: {unitName[maxPriorityindex]},
                Report Type: {reportType[maxPriorityindex]},
                Priority: {priority[maxPriorityindex]},
                Score: {score[maxPriorityindex]}

                """); }
        else
        {
            Console.WriteLine("No approved reports found.\n");
        }

    }


    //helper func for the logic to calculate average by priority
    static string CalculateAverageScoreByPriority(int[] priorityList, int priority, double[] score, int numOfReports)
    {
        double average = 0;
        double count = 0;
        for (int i = 0; i < numOfReports; i++)
        {
            if (priorityList[i] == priority)
            {
                average += score[i];
                count += 1;
            }
        }
        return (count > 0) ? (average / count).ToString("F2") : "No reports."; 
    }


    //display the average score by proirity
    static void DisplayAverageByPriority(int[] priority, double[] score, int numOfReports)
    {
        Console.WriteLine($"""
            ===Display Average Score for each Priority===
            Priority 1 Average Score: {CalculateAverageScoreByPriority(priority, 1, score, numOfReports)}
            Priority 2 Average Score: {CalculateAverageScoreByPriority(priority, 2, score, numOfReports)}
            Priority 3 Average Score: {CalculateAverageScoreByPriority(priority, 3, score, numOfReports)}
            Priority 4 Average Score: {CalculateAverageScoreByPriority(priority, 4, score, numOfReports)}
            Priority 5 Average Score: {CalculateAverageScoreByPriority(priority, 5, score, numOfReports)}

            """);
    }

    static void Main()
    {
        const string path = "C:\\Users\\Yonil\\kodkod_megamot_11\\megamot_project\\megamot_project\\reports.txt";
        const int MAX_REPORTS = 100;
        string[] unitName = new string[MAX_REPORTS];
        ReportType[] reportType = new ReportType[MAX_REPORTS];
        int[] priority = new int[MAX_REPORTS];
        double[] score = new double[MAX_REPORTS];
        Status[] status = new Status[MAX_REPORTS];
        string[] data = LoadFile(path, MAX_REPORTS);
        if (data == null)
        {
            Console.WriteLine($"Error: File {Path.GetFileName(path)} not found.");
            return;
        }
        int numOfValidReports = ProcessReports(unitName, reportType, priority, score, status, data);
        Console.WriteLine($"Processing complete.\nValid records: {numOfValidReports}.\nInvalid records: {data.Length - numOfValidReports}");
        DisplayBasicStatistics(score, numOfValidReports);
        DisplayStatusCounts(status, numOfValidReports);
        DisplayTypeCounts(reportType, numOfValidReports);
        DisplayHighestPriorityApproved(unitName, reportType, priority, score, status, numOfValidReports);
        DisplayAverageByPriority(priority, score, numOfValidReports);
    }
} 