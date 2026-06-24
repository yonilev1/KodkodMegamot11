using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.NetworkInformation;

namespace megamot11;
enum ReportType
{
    COLLECT,
    ANALYZE,
    RECON,
    INTEL
}

enum Status
{
    PENDING,
    APPROVED,
    REJECTED
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

        return validateReport(line[1]) && validatePriority(line[2]) && validateScore(line[3]) && validateStatus(line[4]);
    }


    //validate report - helper function
    static bool validateReport(string report)
    {
        report = report.Trim();
        report = report.ToUpper();
        if (!Enum.TryParse(report, out ReportType reportInEnum))
        {
            return false;
        }
        return true;
    }


    //validate status - helper function
    static bool validateStatus(string status)
    {
        status = status.Trim();
        status = status.ToUpper();
        if (!Enum.TryParse(status, out Status statusInEnum))
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
        rt = rt.ToUpper();
        return Enum.Parse<ReportType>(rt);
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
        st = st.ToUpper();
        return Enum.Parse<Status>(st);
    }


    //add each part of the line to its string. in the right index
    static bool addLineToStatisticsDb(int index, string[] line, string[] unitName, ReportType[] reportType, int[] priority, double[] score, Status[] status)
    {
        if (validateData(line))
        {
            unitName[index] = line[0];
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
        int index = 0;
        foreach (string[] line in spliteLines)
        {
            bool did_add = addLineToStatisticsDb(index, line, unitName, reportType, priority, score, status);
            if (did_add)
            {
                index++;
            }
        }

        int count = 0;
        for(int i = 0; i < unitName.Length; i++)
        {
            if (unitName[i] != null)
            {
                count++;
            }
            else
            {
                break;
            }
        }
        return count;
    }


    static double CalculateAverage(double[] score, int numOfReports)
    {
        double sum = 0;
        for(int i = 0; i < numOfReports; i++)
        {
            sum += score[i];
        }
        return sum / numOfReports;
    }

    static double FindMaxScore(double[] score, int numOfReports)
    {
        double maxScore = 0;
        for (int i = 0; i < numOfReports; i++)
        {
            if (score[i] > maxScore)
            {
                maxScore = score[i];
            }
        }
        return maxScore;
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
        int procreports = ProcessReports(unitName, reportType, priority, score, status, data);
        Console.WriteLine($"Processing complete.\nValid records: {procreports}.\nInvalid records: {data.Length - procreports}");
        Console.WriteLine(CalculateAverage(score, procreports));
        Console.WriteLine(FindMaxScore(score, procreports));

    }
} 