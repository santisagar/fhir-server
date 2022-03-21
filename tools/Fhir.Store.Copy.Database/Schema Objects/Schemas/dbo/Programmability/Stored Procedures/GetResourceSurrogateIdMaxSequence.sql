--DROP PROCEDURE dbo.GetResourceSurrogateIdMaxSequence
GO
CREATE PROCEDURE dbo.GetResourceSurrogateIdMaxSequence @Count int, @MaxSequence bigint OUT
AS
set nocount on
DECLARE @SP varchar(100) = 'GetResourceSurrogateIdMaxSequence'
       ,@Mode varchar(100) = 'Cnt='+convert(varchar,@Count)
       ,@st datetime2 = sysdatetime()
       ,@LastValueVar sql_variant

BEGIN TRY
  --UPDATE dbo.ResourceSurrogateIdMaxSequence
  --  SET MaxSequence = MaxSequence + @Count
  --     ,@MaxSequence = MaxSequence + @Count
  --SET @Rows = @@rowcount

  --IF @Rows <> 1 RAISERROR('Unexpected number of rows in dbo.ResourceSurrogateIdMaxSequence table.', 18, 127)

  -- Below logic is SQL implementation of current C# surrogate id helper.
  -- I don't like it  because it is not full proof, and, unlike commented out above, can produce identical ids for different calls.
  EXECUTE sys.sp_sequence_get_range @sequence_name = 'dbo.ResourceSurrogateIdUniquifierSequence', @range_size = @Count, @range_first_value = NULL, @range_last_value = @LastValueVar OUT
  
  SET @MaxSequence = (datediff_big(microsecond,'00010101',@st)*10+(datepart(nanosecond,@st)%1000)/100)*8 + convert(int,@LastValueVar)

  EXECUTE dbo.LogEvent @Process=@SP,@Mode=@Mode,@Status='End',@Start=@st,@Rows=NULL,@Text=@MaxSequence
END TRY
BEGIN CATCH
  IF error_number() = 1750 THROW -- Real error is before 1750, cannot trap in SQL.
  EXECUTE dbo.LogEvent @Process=@SP,@Mode=@Mode,@Status='Error'
END CATCH
GO
--DECLARE @MaxSequence bigint
--EXECUTE dbo.GetResourceSurrogateIdMaxSequence2 @Count = 500, @MaxSequence = @MaxSequence OUT
--SELECT @MaxSequence