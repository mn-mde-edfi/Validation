/*

When Course grade level offered are within grades K-8, If E is selected, then NOT B, G, X, A, C, D

*/

DECLARE @RuleId VARCHAR(32) = '50.02.0011';
DECLARE @Message NVARCHAR(MAX) = 'Course level type combination error detected for course offered for grades K-8. if course level type E is selected for the course, B, G, X, A, C, or D cannot also be selected on the same course.';
DECLARE @IsError BIT = 1;

WITH 
course_level_types AS (

SELECT
 CourseLevelType.EducationOrganizationId,
 CourseLevelType.CourseCode,
 CourseLevel.CodeValue AS CourseLevelCodeValue
FROM 
 mn.CourseLevelType
 INNER JOIN 
 edfi.Descriptor CourseLevel
  ON CourseLevelTypeDescriptorId = CourseLevel.DescriptorId
 INNER JOIN
 edfi.CourseOfferedGradeLevel
  ON CourseOfferedGradeLevel.EducationOrganizationId = CourseLevelType.EducationOrganizationId
   AND CourseOfferedGradeLevel.CourseCode = CourseLevelType.CourseCode
 INNER JOIN
 edfi.GradeLevelDescriptor
  ON GradeLevelDescriptor.GradeLevelDescriptorId = CourseOfferedGradeLevel.GradeLevelDescriptorId
 INNER JOIN
 edfi.Descriptor GradeLevel
  ON GradeLevel.DescriptorId = GradeLevelDescriptor.GradeLevelDescriptorId
WHERE
 CourseLevelType.EducationOrganizationId = @DistrictId
 AND GradeLevel.CodeValue IN ('HK','KA','KB','KC','KD','KE','KF','KG','KI','KJ','KK','1','2','3','4','5','6','7','8')
GROUP BY
 CourseLevelType.EducationOrganizationId,
 CourseLevelType.CourseCode,
 CourseLevel.CodeValue

 ),
failed_rows AS (

SELECT
 DISTINCT CourseCode
FROM
 course_level_types
WHERE
 CourseLevelCodeValue = 'E'
 AND EXISTS (
  SELECT 1
  FROM
   course_level_types inside
  WHERE
   inside.EducationOrganizationId = course_level_types.EducationOrganizationId
   AND inside.CourseCode = course_level_types.CourseCode
   AND inside.CourseLevelCodeValue IN ('B', 'G', 'X', 'A', 'C', 'D')
   )

)
INSERT INTO 
 rules.RuleValidationDetail (RuleValidationId, Id, RuleId, IsError, [Message])
SELECT TOP 1
 @RuleValidationId, @DistrictId, @RuleId RuleId, @IsError IsError, 
 @Message + (
  SELECT TOP 1 
   ' Course Code: '+CourseCode
  FROM failed_rows
  ) [Message]
FROM failed_rows;