/*
	This is the orders part of the Data.
	This is accessed using Data.Orders
	EG: Orders[] _ = Data.Orders.GettAllOrdersForCustomer(12); // 12 = a customer id. 
*/
using System;
using System.Data;
using Dapper;

namespace UtemorgonPOS.Core;

public static partial class Data
{
	public static class COrders
	{
		// TODO: Add things like GetAllOrdersForCustomer(int customerId)
		public static void CreateOrder(int customerId, DateTime creationDate)
		{
			using var db = GetConnectionObject();
			string sql = """
				INSERT INTO COrder(CustomerId, CreationDate)
				VALUES(@CustomerId, @CreationDate)
				""";
			db.Execute(
				sql, new { CustomerId = customerId, CreationDate = creationDate }
			);

		}
		public static COrder[] GetAll()
		{
			using var db = GetConnectionObject();
			string procedure =
			"""
			CREATE PROCEDURE IF NOT EXISTS GetAllFromCOrder()
				BEGIN 
					SELECT * FROM COrder;
				END;
			""";
			db.Execute(procedure);
			return [.. db.Query<COrder>("GetAllFromCOrder",
				commandType: CommandType.StoredProcedure)];
		}
		public static COrder? GetFromOrderId(int orderId)
		{
			using var db = GetConnectionObject();

			return db.QuerySingleOrDefault<COrder>(
			"""
			 SELECT * FROM COrder
			 WHERE Id = @orderId
			 LIMIT 1;
			""", new { orderId });
		}

		public static COrder[] GetFromDate(DateOnly creationdate)
		{
			using var db = GetConnectionObject();
			return [.. db.Query<COrder>("Select * from COrder where CAST(CreationDate AS DATE) = @CreationDate ", new { CreationDate = creationdate })];
		}
		public static COrder[] GetLate()
		{
			using var db = GetConnectionObject();
			string Query =
			"""
				SELECT CO.*, O.*
				FROM COrder CO
				JOIN OrderRow O on CO.Id = O.COrderId
				JOIN RentalRow RR on O.Id = RR.OrderRowId
				where RR.ReturnedDate > RR.ExpectedReturnDate
				order by CO.Id;
			""";
			var lateReturns = db.Query<COrder, OrderRow, COrder>(Query, (COrder, OrderRow) =>
			{
				COrder.OrderRow = OrderRow;
				return COrder;
			}, splitOn: "Id").AsList().ToArray();
			return lateReturns;
		}

	}
}

//UPDATE ORDER ONLY CUSTOMERID
// Get all unpaid orders
// GETLATE med ett datumspann startdatum till slutdatum

/*

*/